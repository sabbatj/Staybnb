using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Staybnb.Web.Constants;
using Staybnb.Web.Data;
using Staybnb.Web.Models;
using Staybnb.Web.Services;
using Xunit;

namespace Staybnb.Tests;

public class UserRoleServiceTests
{
    private static async Task<(ServiceProvider provider, string guestId)> SetupGuestAsync(string dbName)
    {
        var provider = TestServiceProviderFactory.Create(dbName);
        using var scope = provider.CreateScope();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var roleName in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
                await roleManager.CreateAsync(new IdentityRole(roleName));
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var guest = new ApplicationUser
        {
            UserName = "guest@test.local",
            Email = "guest@test.local",
            FirstName = "Test",
            LastName = "Guest"
        };
        await userManager.CreateAsync(guest, "Password1!");
        await userManager.AddToRoleAsync(guest, Roles.Guest);

        return (provider, guest.Id);
    }

    [Fact]
    public async Task PromoteGuestToHostAsync_AddsHostRole_AndRemovesGuestRole()
    {
        var (provider, guestId) = await SetupGuestAsync(nameof(PromoteGuestToHostAsync_AddsHostRole_AndRemovesGuestRole));
        using var scope = provider.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var service = scope.ServiceProvider.GetRequiredService<IUserRoleService>();

        var result = await service.PromoteGuestToHostAsync(guestId);

        Assert.True(result);
        var user = await userManager.FindByIdAsync(guestId);
        var roles = await userManager.GetRolesAsync(user!);

        Assert.Contains(Roles.Host, roles);
        Assert.DoesNotContain(Roles.Guest, roles);
    }

    [Fact]
    public async Task PromoteGuestToHostAsync_LogsActivity()
    {
        var (provider, guestId) = await SetupGuestAsync(nameof(PromoteGuestToHostAsync_LogsActivity));
        using var scope = provider.CreateScope();

        var service = scope.ServiceProvider.GetRequiredService<IUserRoleService>();
        await service.PromoteGuestToHostAsync(guestId);

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hasLog = await context.ActivityLogs.AnyAsync(l => l.UserId == guestId && l.ActivityType == ActivityType.RoleChange);

        Assert.True(hasLog);
    }

    [Fact]
    public async Task PromoteGuestToAdminAsync_AddsAdminRole()
    {
        var (provider, guestId) = await SetupGuestAsync(nameof(PromoteGuestToAdminAsync_AddsAdminRole));
        using var scope = provider.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var service = scope.ServiceProvider.GetRequiredService<IUserRoleService>();

        await service.PromoteGuestToAdminAsync(guestId);

        var user = await userManager.FindByIdAsync(guestId);
        var roles = await userManager.GetRolesAsync(user!);

        Assert.Contains(Roles.Admin, roles);
    }
}
