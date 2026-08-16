namespace Staybnb.Web.Services;

public interface IUserRoleService
{
    Task<bool> PromoteGuestToHostAsync(string userId);
    Task<bool> PromoteGuestToAdminAsync(string userId);
}
