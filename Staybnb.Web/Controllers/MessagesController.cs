using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybnb.Web.Data;
using Staybnb.Web.Models;
using Staybnb.Web.Models.ViewModels;

namespace Staybnb.Web.Controllers;

[Authorize]
public class MessagesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public MessagesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private string CurrentUserId => _userManager.GetUserId(User)!;

    public async Task<IActionResult> Inbox()
    {
        var userId = CurrentUserId;

        var partnerIds = await _context.Messages
            .Where(m => m.SenderId == userId || m.ReceiverId == userId)
            .Select(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
            .Distinct()
            .ToListAsync();

        var partners = await _context.Users
            .Where(u => partnerIds.Contains(u.Id))
            .ToListAsync();

        var unreadCounts = await _context.Messages
            .Where(m => m.ReceiverId == userId && !m.IsRead)
            .GroupBy(m => m.SenderId)
            .Select(g => new { SenderId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.SenderId, x => x.Count);

        ViewBag.UnreadCounts = unreadCounts;
        return View(partners);
    }

    public async Task<IActionResult> Conversation(string userId)
    {
        var currentId = CurrentUserId;

        var messages = await _context.Messages
            .Where(m => (m.SenderId == currentId && m.ReceiverId == userId) ||
                        (m.SenderId == userId && m.ReceiverId == currentId))
            .OrderBy(m => m.Timestamp)
            .ToListAsync();

        var unread = messages.Where(m => m.ReceiverId == currentId && !m.IsRead).ToList();
        foreach (var m in unread) m.IsRead = true;
        if (unread.Any()) await _context.SaveChangesAsync();

        var partner = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        ViewBag.Partner = partner;
        ViewBag.PartnerId = userId;

        return View(messages);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(MessageComposeViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Content)) return RedirectToAction(nameof(Conversation), new { userId = model.ReceiverId });

        _context.Messages.Add(new Message
        {
            SenderId = CurrentUserId,
            ReceiverId = model.ReceiverId,
            Content = model.Content,
            Timestamp = DateTime.UtcNow,
            IsRead = false
        });

        _context.Notifications.Add(new Notification
        {
            UserId = model.ReceiverId,
            Title = "New Message",
            Message = "You have a new message.",
            Type = NotificationType.NewMessage
        });

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Conversation), new { userId = model.ReceiverId });
    }
}
