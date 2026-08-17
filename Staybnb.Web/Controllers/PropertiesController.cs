using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybnb.Web.Data;
using Staybnb.Web.Models;

namespace Staybnb.Web.Controllers;

public class PropertiesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public PropertiesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? city, string? propertyType)
    {
        var query = _context.HostProperties
            .Include(p => p.Images)
            .Include(p => p.Host)
            .Include(p => p.Reviews)
            .Where(p => p.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(city))
        {
            query = query.Where(p => p.City != null && p.City.Contains(city));
        }

        if (!string.IsNullOrWhiteSpace(propertyType))
        {
            query = query.Where(p => p.PropertyType != null && p.PropertyType.Contains(propertyType));
        }

        ViewBag.City = city;
        ViewBag.PropertyType = propertyType;

        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            var userId = _userManager.GetUserId(User)!;
            ViewBag.WishlistIds = await _context.WishlistItems
                .Where(w => w.UserId == userId)
                .Select(w => w.PropertyId)
                .ToListAsync();
        }
        else
        {
            ViewBag.WishlistIds = new List<int>();
        }

        var properties = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
        return View(properties);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var property = await _context.HostProperties
            .Include(p => p.Images)
            .Include(p => p.Host)
            .Include(p => p.Reviews)
            .Include(p => p.Amenities)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (property == null) return NotFound();

        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            var userId = _userManager.GetUserId(User)!;
            ViewBag.IsWishlisted = await _context.WishlistItems
                .AnyAsync(w => w.UserId == userId && w.PropertyId == id);
        }
        else
        {
            ViewBag.IsWishlisted = false;
        }

        return View(property);
    }
}
