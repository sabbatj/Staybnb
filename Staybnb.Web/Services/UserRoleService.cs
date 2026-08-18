using Microsoft.AspNetCore.Identity;
using Staybnb.Web.Constants;
using Staybnb.Web.Models;

namespace Staybnb.Web.Services;

public class UserRoleService : IUserRoleService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IActivityLogService _activityLog;

    public UserRoleService(UserManager<ApplicationUser> userManager, IActivityLogService activityLog)
    {
        _userManager = userManager;
        _activityLog = activityLog;
    }

    public async Task<bool> PromoteGuestToHostAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        if (await _userManager.IsInRoleAsync(user, Roles.Guest))
        {
            await _userManager.RemoveFromRoleAsync(user, Roles.Guest);
        }

        if (!await _userManager.IsInRoleAsync(user, Roles.Host))
        {
            await _userManager.AddToRoleAsync(user, Roles.Host);
        }

        await _activityLog.LogAsync(userId, "Promoted from Guest to Host", ActivityType.RoleChange);
        return true;
    }

    public async Task<bool> PromoteGuestToAdminAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        if (await _userManager.IsInRoleAsync(user, Roles.Guest))
        {
            var removeGuestResult = await _userManager.RemoveFromRoleAsync(user, Roles.Guest);

            if (!removeGuestResult.Succeeded)
                return false;
        }

        if (!await _userManager.IsInRoleAsync(user, Roles.Admin))
        {
            var addAdminResult = await _userManager.AddToRoleAsync(user, Roles.Admin);

            if (!addAdminResult.Succeeded)
                return false;
        }

        await _activityLog.LogAsync(
            userId,
            "Promoted to Admin by SuperAdmin",
            ActivityType.RoleChange);

        return true;
    }
}
