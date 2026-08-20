using System.Text.Json;
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

[Authorize(Roles = Roles.Host)]
public class HostController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IActivityLogService _activityLog;
    private readonly IWebHostEnvironment _env;

    public HostController(
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

    public async Task<IActionResult> Dashboard()
    {
        var hostId = CurrentUserId;

        var propertyIds = await _context.HostProperties
            .Where(p => p.HostId == hostId)
            .Select(p => p.Id)
            .ToListAsync();

        ViewBag.TotalProperties = propertyIds.Count;
        ViewBag.ActiveProperties = await _context.HostProperties.CountAsync(p => p.HostId == hostId && p.IsActive);

        var hostBookings = await _context.Bookings
            .Where(b => propertyIds.Contains(b.PropertyId))
            .ToListAsync();

        ViewBag.PendingBookings = hostBookings.Count(b => b.Status == BookingStatus.Pending);
        ViewBag.TotalBookings = hostBookings.Count;

        var confirmedBookings = hostBookings
            .Where(b => b.Status == BookingStatus.Approved || b.Status == BookingStatus.CheckedIn || b.Status == BookingStatus.Completed)
            .ToList();

        ViewBag.TotalEarnings = confirmedBookings.Sum(b => b.TotalPrice);
        ViewBag.AverageBookingValue = confirmedBookings.Any() ? confirmedBookings.Average(b => b.TotalPrice) : 0;

        var bookingsThisMonth = hostBookings.Count(b => b.CreatedAt.Month == DateTime.UtcNow.Month && b.CreatedAt.Year == DateTime.UtcNow.Year);
        ViewBag.BookingsThisMonth = bookingsThisMonth;

        var reviews = await _context.Reviews
            .Where(r => propertyIds.Contains(r.PropertyId))
            .ToListAsync();

        ViewBag.AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
        ViewBag.TotalReviews = reviews.Count;

        ViewBag.UnreadMessages = await _context.Messages
            .CountAsync(m => m.ReceiverId == hostId && !m.IsRead);

        ViewBag.UnreadNotifications = await _context.Notifications
            .CountAsync(n => n.UserId == hostId && !n.IsRead);

        var occupancyRate = ViewBag.TotalProperties > 0
            ? Math.Round((double)ViewBag.ActiveProperties / ViewBag.TotalProperties * 100, 0)
            : 0;
        ViewBag.OccupancyRate = occupancyRate;

        return View();
    }

    public async Task<IActionResult> MyProperties()
    {
        var properties = await _context.HostProperties
            .Include(p => p.Images)
            .Where(p => p.HostId == CurrentUserId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return View(properties);
    }

    [HttpGet]
    public IActionResult AddProperty() => View(new AddPropertyViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddProperty(AddPropertyViewModel model)
    {
        if (model.Images == null || model.Images.Count == 0 || model.Images.All(f => f.Length == 0))
        {
            ModelState.AddModelError(nameof(model.Images), "At least one property image is required.");
        }
        else
        {
            foreach (var file in model.Images.Where(f => f != null && f.Length > 0))
            {
                if (!FileUploadValidationService.IsValidImage(file))
                {
                    ModelState.AddModelError(
                        nameof(model.Images),
                        "Each property image must be a JPG, JPEG, PNG, or WEBP file no larger than 5 MB.");
                }
            }
        }

        if (!ModelState.IsValid) return View(model);

        var property = new HostProperty
        {
            Title = model.Title,
            Description = model.Description,
            PricePerNight = model.PricePerNight,
            HostId = CurrentUserId,
            Address = model.Address,
            City = model.City,
            PropertyType = model.PropertyType,
            MaxGuests = model.MaxGuests,
            Bedrooms = model.Bedrooms,
            Beds = model.Beds,
            Bathrooms = model.Bathrooms,
            CleaningFee = model.CleaningFee,
            ServiceFee = model.ServiceFee,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.HostProperties.Add(property);
        await _context.SaveChangesAsync();

        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "properties");
        Directory.CreateDirectory(uploadsFolder);

        var imagesToSave = model.Images?.Where(f => f.Length > 0) ?? Enumerable.Empty<IFormFile>();
        foreach (var file in imagesToSave)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            _context.PropertyImages.Add(new PropertyImage
            {
                PropertyId = property.Id,
                ImageUrl = $"/uploads/properties/{fileName}"
            });
        }

        await _context.SaveChangesAsync();
        await _activityLog.LogAsync(CurrentUserId, $"Added new property '{property.Title}'", ActivityType.PropertyChange);

        return RedirectToAction(nameof(MyProperties));
    }

    [HttpGet]
    public async Task<IActionResult> EditProperty(int id)
    {
        var property = await _context.HostProperties
            .FirstOrDefaultAsync(p => p.Id == id && p.HostId == CurrentUserId);

        if (property == null) return NotFound();

        var model = new EditPropertyViewModel
        {
            Id = property.Id,
            Title = property.Title,
            Description = property.Description,
            PricePerNight = property.PricePerNight,
            Address = property.Address,
            City = property.City,
            PropertyType = property.PropertyType,
            MaxGuests = property.MaxGuests,
            Bedrooms = property.Bedrooms,
            Beds = property.Beds,
            Bathrooms = property.Bathrooms,
            CleaningFee = property.CleaningFee,
            ServiceFee = property.ServiceFee
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProperty(EditPropertyViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var property = await _context.HostProperties
            .FirstOrDefaultAsync(p => p.Id == model.Id && p.HostId == CurrentUserId);

        if (property == null) return NotFound();

        property.Title = model.Title;
        property.Description = model.Description;
        property.PricePerNight = model.PricePerNight;
        property.Address = model.Address;
        property.City = model.City;
        property.PropertyType = model.PropertyType;
        property.MaxGuests = model.MaxGuests;
        property.Bedrooms = model.Bedrooms;
        property.Beds = model.Beds;
        property.Bathrooms = model.Bathrooms;
        property.CleaningFee = model.CleaningFee;
        property.ServiceFee = model.ServiceFee;

        await _context.SaveChangesAsync();
        await _activityLog.LogAsync(CurrentUserId, $"Edited property '{property.Title}'", ActivityType.PropertyChange);

        return RedirectToAction(nameof(MyProperties));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var property = await _context.HostProperties
            .FirstOrDefaultAsync(p => p.Id == id && p.HostId == CurrentUserId);

        if (property == null) return NotFound();

        property.IsActive = !property.IsActive;
        await _context.SaveChangesAsync();

        await _activityLog.LogAsync(CurrentUserId, $"Toggled '{property.Title}' active status to {property.IsActive}", ActivityType.PropertyChange);

        return RedirectToAction(nameof(MyProperties));
    }

    public async Task<IActionResult> BookingRequests()
    {
        var bookings = await _context.Bookings
            .Include(b => b.Property)!.ThenInclude(p => p!.Images)
            .Include(b => b.Guest)
            .Where(b => b.Property!.HostId == CurrentUserId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return View(bookings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateBookingStatus(int id, string status)
    {
        var booking = await _context.Bookings
            .Include(b => b.Property)
            .Include(b => b.Guest)
            .FirstOrDefaultAsync(b => b.Id == id && b.Property!.HostId == CurrentUserId);

        if (booking == null) return NotFound();

        if (Enum.TryParse<BookingStatus>(status, out var newStatus))
        {
            booking.Status = newStatus;
            await _context.SaveChangesAsync();

            _context.Notifications.Add(new Notification
            {
                UserId = booking.GuestId,
                Title = $"Booking {newStatus}",
                Message = $"Your booking for '{booking.Property?.Title}' is now {newStatus}.",
                Type = NotificationType.BookingUpdate,
                ActionUrl = Url.Action("MyBookings", "Booking")
            });
            await _context.SaveChangesAsync();

            await _activityLog.LogAsync(CurrentUserId, $"Booking #{id} status changed to {newStatus}", ActivityType.BookingStatusUpdate);
        }

        return RedirectToAction(nameof(BookingRequests));
    }

    [HttpGet]
    public async Task<IActionResult> CheckInProcess(int propertyId)
    {
        var property = await _context.HostProperties
            .Include(p => p.CheckInProcess)
            .FirstOrDefaultAsync(p => p.Id == propertyId && p.HostId == CurrentUserId);

        if (property == null) return NotFound();

        var requiredDocuments = property.CheckInProcess != null
            ? JsonSerializer.Deserialize<List<string>>(
                property.CheckInProcess.RequiredDocumentsJson) ?? new List<string>()
            : new List<string>();

        var model = new CheckInProcessViewModel
        {
            PropertyId = property.Id,
            Title = property.CheckInProcess?.Title ?? "Check-in Instructions",
            StepsText = property.CheckInProcess != null
                ? string.Join("\n",
                    JsonSerializer.Deserialize<List<string>>(property.CheckInProcess.StepsJson) ?? new())
                : string.Empty,
            RequireId = requiredDocuments.Contains("ID"),
            RequirePassport = requiredDocuments.Contains("Passport")
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckInProcess(CheckInProcessViewModel model)
    {
        var property = await _context.HostProperties
            .Include(p => p.CheckInProcess)
            .FirstOrDefaultAsync(p => p.Id == model.PropertyId && p.HostId == CurrentUserId);

        if (property == null) return NotFound();

        var steps = model.StepsText
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        var stepsJson = JsonSerializer.Serialize(steps);

        var requiredDocuments = new List<string>();

        if (model.RequireId)
            requiredDocuments.Add("ID");

        if (model.RequirePassport)
            requiredDocuments.Add("Passport");

        var requiredDocumentsJson = JsonSerializer.Serialize(requiredDocuments);

        if (property.CheckInProcess == null)
        {
            _context.CheckInProcesses.Add(new CheckInProcess
            {
                PropertyId = property.Id,
                Title = model.Title,
                StepsJson = stepsJson,
                RequiredDocumentsJson = requiredDocumentsJson,
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            property.CheckInProcess.Title = model.Title;
            property.CheckInProcess.StepsJson = stepsJson;
            property.CheckInProcess.RequiredDocumentsJson = requiredDocumentsJson;
        }

        await _context.SaveChangesAsync();
        await _activityLog.LogAsync(
            CurrentUserId,
            $"Configured check-in process for '{property.Title}'",
            ActivityType.PropertyChange);

        return RedirectToAction(nameof(MyProperties));
    }

    public async Task<IActionResult> CheckIns()
    {
        var checkIns = await _context.GuestCheckIns
            .Include(g => g.Booking)!.ThenInclude(b => b!.Property)
            .Include(g => g.Booking)!.ThenInclude(b => b!.Guest)
            .Include(g => g.Documents)
            .Where(g => g.Booking!.Property!.HostId == CurrentUserId)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

        return View(checkIns);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyDocument(int documentId)
    {
        var document = await _context.GuestDocuments
            .Include(d => d.GuestCheckIn)!.ThenInclude(g => g!.Booking)!.ThenInclude(b => b!.Property)
            .FirstOrDefaultAsync(d => d.Id == documentId);

        if (document == null || document.GuestCheckIn?.Booking?.Property?.HostId != CurrentUserId)
        {
            return NotFound();
        }

        document.Status = DocumentStatus.Verified;

        if (document.GuestCheckIn != null)
        {
            var checkIn = document.GuestCheckIn;

            // ID OR Passport is sufficient for this requirement.
            var hasVerifiedIdentity = checkIn.Documents.Any(d =>
                (string.Equals(d.DocumentType, "ID", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(d.DocumentType, "Passport", StringComparison.OrdinalIgnoreCase)) &&
                d.Status == DocumentStatus.Verified);

            if (hasVerifiedIdentity)
            {
                checkIn.Status = CheckInStatus.Verified;

                if (checkIn.Booking != null)
                {
                    checkIn.Booking.Status = BookingStatus.CheckedIn;
                    checkIn.Booking.CheckedInAt = DateTime.UtcNow;
                }
            }
        }

        await _context.SaveChangesAsync();
        await _activityLog.LogAsync(CurrentUserId, $"Verified guest document #{documentId}", ActivityType.Other);

        return RedirectToAction(nameof(CheckIns));
    }
}
