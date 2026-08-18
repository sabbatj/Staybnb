using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Staybnb.Web.Constants;
using Staybnb.Web.Models;
using Staybnb.Web.Services;
using Xunit;

namespace Staybnb.Tests;

public class UserRoleServiceTests
{
    private static ServiceProvider CreateProvider(string dbName)
    {
        return TestServiceProviderFactory.Create(dbName);
    }

    [Fact]
    public async Task PromoteGuestToHost_RemovesGuestRole_AndAddsHostRole()
    {
        using var provider = CreateProvider(nameof(PromoteGuestToHost_RemovesGuestRole_AndAddsHostRole));

        using var scope = provider.CreateScope();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        var service = scope.ServiceProvider
            .GetRequiredService<IUserRoleService>();

        var user = new ApplicationUser
        {
            UserName = "flow.guest@test.local",
            Email = "flow.guest@test.local",
            FirstName = "Flow",
            LastName = "Guest",
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, "TestPassword123!");

        Assert.True(result.Succeeded);

        await userManager.AddToRoleAsync(user, Roles.Guest);

        await service.PromoteGuestToHostAsync(user.Id);

        var roles = await userManager.GetRolesAsync(user);

        Assert.Contains(Roles.Host, roles);
        Assert.DoesNotContain(Roles.Guest, roles);
    }

    [Fact]
    public async Task PromoteGuestToAdmin_AddsAdminRole_AndRemovesGuestRole()
    {
        using var provider = CreateProvider(nameof(PromoteGuestToAdmin_AddsAdminRole_AndRemovesGuestRole));

        using var scope = provider.CreateScope();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        var service = scope.ServiceProvider
            .GetRequiredService<IUserRoleService>();

        var user = new ApplicationUser
        {
            UserName = "admin.flow@test.local",
            Email = "admin.flow@test.local",
            FirstName = "Admin",
            LastName = "Flow",
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, "TestPassword123!");

        Assert.True(result.Succeeded);

        await userManager.AddToRoleAsync(user, Roles.Guest);

        await service.PromoteGuestToAdminAsync(user.Id);

        var roles = await userManager.GetRolesAsync(user);

        Assert.Contains(Roles.Admin, roles);
        Assert.DoesNotContain(Roles.Guest, roles);
    }
}
