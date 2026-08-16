using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybnb.Web.Data;
using Staybnb.Web.Models.ViewModels;

namespace Staybnb.Web.Controllers;

public class PropertiesController : Controller
{
    private readonly ApplicationDbContext _context;

    public PropertiesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? city, string? propertyType)
    {
        var query = _context.HostProperties
            .Include(p => p.Images)
            .Include(p => p.Host)
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

        return View(property);
    }
}
