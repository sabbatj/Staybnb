using Staybnb.Web.Models;

namespace Staybnb.Web.Services;

public interface IActivityLogService
{
    Task LogAsync(string userId, string action, ActivityType type);
}
