using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Staybnb.Web.Constants;
using Staybnb.Web.Models;
using Staybnb.Web.Services;
using Xunit;

namespace Staybnb.Tests;

public class HostRoleTransitionTests
{
    [Fact]
    public async Task PromoteGuestToHost_RemovesGuestRole()
    {
        var provider = TestServiceProviderFactory.Create(
            nameof(PromoteGuestToHost_RemovesGuestRole));

        using var scope = provider.CreateScope();

        var userManager =
            scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var roleManager =
            scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        var service =
            scope.ServiceProvider.GetRequiredService<IUserRoleService>();

        foreach (var role in new[] { Roles.Guest, Roles.Host })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var user = new ApplicationUser
        {
            UserName = "guest-host@test.local",
            Email = "guest-host@test.local",
            FirstName = "Guest",
            LastName = "Host",
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(
            user,
            "TestPassword123!");

        Assert.True(result.Succeeded);

        await userManager.AddToRoleAsync(user, Roles.Guest);

        await service.PromoteGuestToHostAsync(user.Id);

        var roles = await userManager.GetRolesAsync(user);

        Assert.Contains(Roles.Host, roles);
        Assert.DoesNotContain(Roles.Guest, roles);
    }
}
