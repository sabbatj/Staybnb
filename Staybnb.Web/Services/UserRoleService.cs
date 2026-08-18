using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Staybnb.Web.Constants;
using Staybnb.Web.Data;
using Staybnb.Web.Models;

namespace Staybnb.Web.Services;

public class UserRoleService : IUserRoleService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IActivityLogService _activityLog;
    private readonly ApplicationDbContext _context;

    public UserRoleService(
        UserManager<ApplicationUser> userManager,
        IActivityLogService activityLog,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _activityLog = activityLog;
        _context = context;
    }

    public async Task<bool> PromoteGuestToHostAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        if (await _userManager.IsInRoleAsync(user, Roles.Guest))
        {
            var removeGuestResult =
                await _userManager.RemoveFromRoleAsync(user, Roles.Guest);

            if (!removeGuestResult.Succeeded)
                return false;
        }

        if (!await _userManager.IsInRoleAsync(user, Roles.Host))
        {
            var addHostResult =
                await _userManager.AddToRoleAsync(user, Roles.Host);

            if (!addHostResult.Succeeded)
                return false;
        }

        await _activityLog.LogAsync(
            userId,
            "Promoted from Guest to Host",
            ActivityType.RoleChange);

        return true;
    }

    public async Task<bool> PromoteGuestToAdminAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        if (await _userManager.IsInRoleAsync(user, Roles.Guest))
        {
            var removeGuestResult =
                await _userManager.RemoveFromRoleAsync(user, Roles.Guest);

            if (!removeGuestResult.Succeeded)
                return false;
        }

        if (!await _userManager.IsInRoleAsync(user, Roles.Admin))
        {
            var addAdminResult =
                await _userManager.AddToRoleAsync(user, Roles.Admin);

            if (!addAdminResult.Succeeded)
                return false;
        }

        await _activityLog.LogAsync(
            userId,
            "Promoted to Admin by SuperAdmin",
            ActivityType.RoleChange);

        return true;
    }

    public async Task<bool> RemoveHostAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return false;

        if (!await _userManager.IsInRoleAsync(user, Roles.Host))
            return false;

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            // Deactivate every property owned by this host.
            var properties = await _context.HostProperties
                .Where(p => p.HostId == userId && p.IsActive)
                .ToListAsync();

            foreach (var property in properties)
            {
                property.IsActive = false;
            }

            await _context.SaveChangesAsync();

            // Remove Host role.
            var removeHostResult =
                await _userManager.RemoveFromRoleAsync(user, Roles.Host);

            if (!removeHostResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return false;
            }

            // A normal former host should return to Guest access.
            // Do not add Guest if this person is also an Admin.
            var isAdmin =
                await _userManager.IsInRoleAsync(user, Roles.Admin);

            var isGuest =
                await _userManager.IsInRoleAsync(user, Roles.Guest);

            if (!isAdmin && !isGuest)
            {
                var addGuestResult =
                    await _userManager.AddToRoleAsync(user, Roles.Guest);

                if (!addGuestResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            }

            _context.Notifications.Add(new Notification
            {
                UserId = userId,
                Title = "Host Access Removed",
                Message =
                    "Your Host status has been removed by an administrator. " +
                    "Your properties are no longer active.",
                Type = NotificationType.HostApplicationUpdate
            });

            await _context.SaveChangesAsync();

            await _activityLog.LogAsync(
                userId,
                "Host access removed by Admin",
                ActivityType.RoleChange);

            await transaction.CommitAsync();

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }
}
