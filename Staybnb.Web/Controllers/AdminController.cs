using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Staybnb.Web.Constants;
using Staybnb.Web.Data;
using Staybnb.Web.Models;
using Staybnb.Web.Services;

namespace Staybnb.Web.Controllers;

[Authorize(Roles = Roles.Admin)]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserRoleService _userRoleService;
    private readonly IActivityLogService _activityLog;

    public AdminController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IUserRoleService userRoleService,
        IActivityLogService activityLog)
    {
        _context = context;
        _userManager = userManager;
        _userRoleService = userRoleService;
        _activityLog = activityLog;
    }

    [AllowAnonymous]
    public async Task<IActionResult> DebugListUsers()
    {
        var users = await _userManager.Users.ToListAsync();
        var result = new List<object>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            result.Add(new { u.Email, u.FirstName, u.LastName, Roles = string.Join(", ", roles), u.CreatedAt });
        }
        return Json(result);
    }

    public async Task<IActionResult> Dashboard()
    {
        ViewBag.TotalUsers = await _context.Users.CountAsync();
        ViewBag.PendingApps = await _context.HostApplications.CountAsync(a => a.Status == ApplicationStatus.Pending);
        ViewBag.ActiveProperties = await _context.HostProperties.CountAsync(p => p.IsActive);
        ViewBag.ApprovedHosts = await _context.HostApplications.CountAsync(a => a.Status == ApplicationStatus.Approved);
        return View();
    }

    public async Task<IActionResult> Applications()
    {
        var applications = await _context.HostApplications
            .Include(a => a.Applicant)
            .Include(a => a.Property)
            .ThenInclude(p => p!.Images)
            .OrderByDescending(a => a.AppliedAt)
            .ToListAsync();

        return View(applications);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveApplication(int id)
    {
        var application = await _context.HostApplications
            .Include(a => a.Property)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (application == null) return NotFound();

        application.Status = ApplicationStatus.Approved;
        if (application.Property != null)
        {
            application.Property.IsActive = true;
        }

        await _context.SaveChangesAsync();

        // Core requirement: add Host role, remove Guest role
        await _userRoleService.PromoteGuestToHostAsync(application.ApplicationUserId);

        _context.Notifications.Add(new Notification
        {
            UserId = application.ApplicationUserId,
            Title = "Host Application Approved",
            Message = $"Congratulations! Your host application for '{application.Property?.Title}' has been approved.",
            Type = NotificationType.HostApplicationUpdate
        });
        await _context.SaveChangesAsync();

        await _activityLog.LogAsync(_userManager.GetUserId(User)!, $"Approved host application #{id}", ActivityType.HostApplication);

        return RedirectToAction(nameof(Applications));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectApplication(int id)
    {
        var application = await _context.HostApplications.FindAsync(id);
        if (application == null) return NotFound();

        application.Status = ApplicationStatus.Rejected;
        await _context.SaveChangesAsync();

        _context.Notifications.Add(new Notification
        {
            UserId = application.ApplicationUserId,
            Title = "Host Application Rejected",
            Message = "Unfortunately your host application was not approved.",
            Type = NotificationType.HostApplicationUpdate
        });
        await _context.SaveChangesAsync();

        await _activityLog.LogAsync(_userManager.GetUserId(User)!, $"Rejected host application #{id}", ActivityType.HostApplication);

        return RedirectToAction(nameof(Applications));
    }

    [Authorize(Roles = Roles.SuperAdmin)]
    public async Task<IActionResult> Users()
    {
        var guests = await _userManager.GetUsersInRoleAsync(Roles.Guest);
        return View(guests);
    }

    [HttpPost]
    [Authorize(Roles = Roles.SuperAdmin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PromoteToAdmin(string id)
    {
        await _userRoleService.PromoteGuestToAdminAsync(id);
        return RedirectToAction(nameof(Users));
    }

    public async Task<IActionResult> ActivityLogs()
    {
        var logs = await _context.ActivityLogs
            .Include(a => a.User)
            .OrderByDescending(a => a.CreatedAt)
            .Take(200)
            .ToListAsync();

        return View(logs);
    }
}
