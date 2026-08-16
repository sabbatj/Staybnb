using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybnb.Web.Constants;
using Staybnb.Web.Data;
using Staybnb.Web.Models;
using Staybnb.Web.Models.ViewModels;
using Staybnb.Web.Services;

namespace Staybnb.Web.Controllers;

[Authorize(Roles = Roles.Guest)]
public class BookingController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IActivityLogService _activityLog;
    private readonly IWebHostEnvironment _env;

    public BookingController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IActivityLogService activityLog,
        IWebHostEnvironment env)
    {
        _context = context;
        _userManager = userManager;
        _activityLog = activityLog;
        _env = env;
    }

    private string CurrentUserId => _userManager.GetUserId(User)!;

    [HttpGet]
    public async Task<IActionResult> Create(int propertyId)
    {
        var property = await _context.HostProperties.FirstOrDefaultAsync(p => p.Id == propertyId && p.IsActive);
        if (property == null) return NotFound();

        ViewBag.Property = property;
        return View(new BookingCreateViewModel { PropertyId = propertyId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingCreateViewModel model)
    {
        var property = await _context.HostProperties.FirstOrDefaultAsync(p => p.Id == model.PropertyId && p.IsActive);
        if (property == null) return NotFound();

        var nights = (model.CheckOutDate - model.CheckInDate).Days;

        if (nights <= 0)
        {
            ModelState.AddModelError(string.Empty, "Check-out date must be after check-in date.");
        }

        if (model.NumberOfGuests > property.MaxGuests)
        {
            ModelState.AddModelError(nameof(model.NumberOfGuests), $"This property allows a maximum of {property.MaxGuests} guests.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Property = property;
            return View(model);
        }

        var totalPrice = (property.PricePerNight * nights) + property.CleaningFee + property.ServiceFee;

        var booking = new Booking
        {
            PropertyId = property.Id,
            GuestId = CurrentUserId,
            CheckInDate = model.CheckInDate,
            CheckOutDate = model.CheckOutDate,
            NumberOfGuests = model.NumberOfGuests,
            TotalPrice = totalPrice,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        _context.Notifications.Add(new Notification
        {
            UserId = property.HostId,
            Title = "New Booking Request",
            Message = $"You have a new booking request for '{property.Title}'.",
            Type = NotificationType.BookingUpdate
        });
        await _context.SaveChangesAsync();

        await _activityLog.LogAsync(CurrentUserId, $"Requested booking for '{property.Title}'", ActivityType.BookingStatusUpdate);

        return RedirectToAction(nameof(MyBookings));
    }

    public async Task<IActionResult> MyBookings()
    {
        var bookings = await _context.Bookings
            .Include(b => b.Property)!.ThenInclude(p => p!.Images)
            .Where(b => b.GuestId == CurrentUserId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return View(bookings);
    }

    [HttpGet]
    public async Task<IActionResult> CheckIn(int bookingId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Property)!.ThenInclude(p => p!.CheckInProcess)
            .Include(b => b.GuestCheckIn)!.ThenInclude(g => g!.Documents)
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.GuestId == CurrentUserId);

        if (booking == null) return NotFound();

        if (booking.Status != BookingStatus.Approved && booking.Status != BookingStatus.CheckedIn)
        {
            TempData["Error"] = "Check-in is only available for approved bookings.";
            return RedirectToAction(nameof(MyBookings));
        }

        return View(booking);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitCheckIn(int bookingId, string documentType, IFormFile document)
    {
        var booking = await _context.Bookings
            .Include(b => b.Property)!.ThenInclude(p => p!.CheckInProcess)
            .Include(b => b.GuestCheckIn)
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.GuestId == CurrentUserId);

        if (booking == null) return NotFound();
        if (booking.Property?.CheckInProcess == null)
        {
            TempData["Error"] = "The host has not configured a check-in process for this property yet.";
            return RedirectToAction(nameof(CheckIn), new { bookingId });
        }

        if (document == null || document.Length == 0)
        {
            TempData["Error"] = "Please upload a document (ID or Passport).";
            return RedirectToAction(nameof(CheckIn), new { bookingId });
        }

        var guestCheckIn = booking.GuestCheckIn;
        if (guestCheckIn == null)
        {
            guestCheckIn = new GuestCheckIn
            {
                BookingId = booking.Id,
                CheckInProcessId = booking.Property.CheckInProcess.Id,
                Status = CheckInStatus.Submitted,
                CreatedAt = DateTime.UtcNow
            };
            _context.GuestCheckIns.Add(guestCheckIn);
            await _context.SaveChangesAsync();
        }
        else
        {
            guestCheckIn.Status = CheckInStatus.Submitted;
        }

        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "documents");
        Directory.CreateDirectory(uploadsFolder);
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(document.FileName)}";
        var filePath = Path.Combine(uploadsFolder, fileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await document.CopyToAsync(stream);
        }

        _context.GuestDocuments.Add(new GuestDocument
        {
            GuestCheckInId = guestCheckIn.Id,
            DocumentType = documentType,
            FileName = fileName,
            Status = DocumentStatus.Pending
        });

        booking.Status = BookingStatus.CheckedIn;

        await _context.SaveChangesAsync();
        await _activityLog.LogAsync(CurrentUserId, $"Completed check-in for booking #{booking.Id}", ActivityType.BookingStatusUpdate);

        return RedirectToAction(nameof(MyBookings));
    }
}
