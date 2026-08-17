using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybnb.Web.Data;
using Staybnb.Web.Models;

namespace Staybnb.Web.Controllers;

[Authorize]
public class WishlistController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public WishlistController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int propertyId, string? returnUrl)
    {
        var userId = _userManager.GetUserId(User)!;

        var existing = await _context.WishlistItems
            .FirstOrDefaultAsync(w => w.UserId == userId && w.PropertyId == propertyId);

        if (existing != null)
        {
            _context.WishlistItems.Remove(existing);
        }
        else
        {
            _context.WishlistItems.Add(new WishlistItem
            {
                UserId = userId,
                PropertyId = propertyId,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction("Index", "Properties");
    }

    public async Task<IActionResult> MyWishlist()
    {
        var userId = _userManager.GetUserId(User)!;

        var items = await _context.WishlistItems
            .Include(w => w.Property)!.ThenInclude(p => p!.Images)
            .Include(w => w.Property)!.ThenInclude(p => p!.Reviews)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();

        return View(items);
    }
}
