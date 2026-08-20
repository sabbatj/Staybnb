namespace Staybnb.Web.Services;

public interface IUserRoleService
{
    Task<bool> PromoteGuestToHostAsync(string userId);
    Task<bool> PromoteGuestToAdminAsync(string userId);
    Task<bool> RemoveHostAsync(string userId);
    Task<bool> DeleteUserAsync(string userId);
}
