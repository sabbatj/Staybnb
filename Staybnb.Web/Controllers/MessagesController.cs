using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybnb.Web.Constants;
using Staybnb.Web.Data;
using Staybnb.Web.Models;
using Staybnb.Web.Models.ViewModels;

namespace Staybnb.Web.Controllers;

[Authorize]
public class MessagesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public MessagesController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
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
            .Select(g => new
            {
                SenderId = g.Key,
                Count = g.Count()
            })
            .ToDictionaryAsync(x => x.SenderId, x => x.Count);

        var latestMessages = await _context.Messages
            .Where(m => m.SenderId == userId || m.ReceiverId == userId)
            .GroupBy(m => m.SenderId == userId
                ? m.ReceiverId
                : m.SenderId)
            .Select(g => g.OrderByDescending(m => m.Timestamp).First())
            .ToListAsync();

        ViewBag.UnreadCounts = unreadCounts;

        ViewBag.LatestMessages = latestMessages.ToDictionary(
            m => m.SenderId == userId
                ? m.ReceiverId
                : m.SenderId);

        return View(partners);
    }

    public async Task<IActionResult> Conversation(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest();

        var currentId = CurrentUserId;

        if (userId == currentId)
            return BadRequest();

        var partner = await _userManager.FindByIdAsync(userId);

        if (partner == null)
            return NotFound();

        var hasExistingConversation = await HasExistingConversation(
            currentId,
            userId);

        var canStartConversation = await CanStartConversation(
            currentId,
            partner);

        if (!hasExistingConversation && !canStartConversation)
            return Forbid();

        var messages = await _context.Messages
            .Where(m =>
                (m.SenderId == currentId && m.ReceiverId == userId) ||
                (m.SenderId == userId && m.ReceiverId == currentId))
            .OrderBy(m => m.Timestamp)
            .ToListAsync();

        var unread = messages
            .Where(m => m.ReceiverId == currentId && !m.IsRead)
            .ToList();

        foreach (var message in unread)
            message.IsRead = true;

        if (unread.Any())
            await _context.SaveChangesAsync();

        ViewBag.Partner = partner;
        ViewBag.PartnerId = userId;

        return View(messages);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(MessageComposeViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.ReceiverId))
            return BadRequest();

        if (model.ReceiverId == CurrentUserId)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            return RedirectToAction(
                nameof(Conversation),
                new { userId = model.ReceiverId });
        }

        if (string.IsNullOrWhiteSpace(model.Content))
        {
            return RedirectToAction(
                nameof(Conversation),
                new { userId = model.ReceiverId });
        }

        if (model.Content.Length > 2000)
        {
            TempData["Error"] = "Message cannot exceed 2000 characters.";

            return RedirectToAction(
                nameof(Conversation),
                new { userId = model.ReceiverId });
        }

        var currentId = CurrentUserId;

        var receiver = await _userManager.FindByIdAsync(model.ReceiverId);

        if (receiver == null)
            return NotFound();

        var hasExistingConversation = await HasExistingConversation(
            currentId,
            receiver.Id);

        var canStartConversation = await CanStartConversation(
            currentId,
            receiver);

        if (!hasExistingConversation && !canStartConversation)
            return Forbid();

        var content = model.Content.Trim();

        if (content.Length == 0)
        {
            TempData["Error"] = "Message cannot be empty.";

            return RedirectToAction(
                nameof(Conversation),
                new { userId = receiver.Id });
        }

        _context.Messages.Add(new Message
        {
            SenderId = currentId,
            ReceiverId = receiver.Id,
            Content = content,
            Timestamp = DateTime.UtcNow,
            IsRead = false
        });

        _context.Notifications.Add(new Notification
        {
            UserId = receiver.Id,
            Title = "New Message",
            Message = "You have a new message.",
            Type = NotificationType.NewMessage,
            ActionUrl = Url.Action("Inbox", "Messages")
        });

        await _context.SaveChangesAsync();

        return RedirectToAction(
            nameof(Conversation),
            new { userId = receiver.Id });
    }

    private async Task<bool> HasExistingConversation(
        string currentUserId,
        string otherUserId)
    {
        return await _context.Messages.AnyAsync(m =>
            (m.SenderId == currentUserId &&
             m.ReceiverId == otherUserId) ||
            (m.SenderId == otherUserId &&
             m.ReceiverId == currentUserId));
    }

    private async Task<bool> CanStartConversation(
        string currentUserId,
        ApplicationUser receiver)
    {
        var currentUser = await _userManager.FindByIdAsync(currentUserId);

        if (currentUser == null)
            return false;

        var senderIsGuest = await _userManager.IsInRoleAsync(
            currentUser,
            Roles.Guest);

        var senderIsHost = await _userManager.IsInRoleAsync(
            currentUser,
            Roles.Host);

        var receiverIsGuest = await _userManager.IsInRoleAsync(
            receiver,
            Roles.Guest);

        var receiverIsHost = await _userManager.IsInRoleAsync(
            receiver,
            Roles.Host);

        // Guests may contact hosts about their properties.
        if (senderIsGuest && receiverIsHost)
            return true;

        // Hosts may only initiate conversations with guests
        // who have a booking for one of the host's properties.
        if (senderIsHost && receiverIsGuest)
        {
            return await _context.Bookings
                .AnyAsync(b =>
                    b.GuestId == receiver.Id &&
                    b.Property != null &&
                    b.Property.HostId == currentUserId);
        }

        return false;
    }
}
