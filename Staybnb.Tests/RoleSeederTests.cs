using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Staybnb.Web.Constants;
using Staybnb.Web.Data;
using Staybnb.Web.Models;
using Xunit;

namespace Staybnb.Tests;

public class RoleSeederTests
{
    [Fact]
    public async Task SeedAsync_CreatesAllFourRoles()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(SeedAsync_CreatesAllFourRoles));
        using var scope = provider.CreateScope();

        await RoleSeeder.SeedAsync(scope.ServiceProvider);

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var roleName in Roles.All)
        {
            Assert.True(await roleManager.RoleExistsAsync(roleName), $"Role '{roleName}' was not created.");
        }
    }

    [Fact]
    public async Task SeedAsync_CreatesSuperAdminUser_WithSuperAdminAndAdminRoles()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(SeedAsync_CreatesSuperAdminUser_WithSuperAdminAndAdminRoles));
        using var scope = provider.CreateScope();

        await RoleSeeder.SeedAsync(scope.ServiceProvider);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var superAdmin = await userManager.FindByEmailAsync("superadmin@staybnb.local");

        Assert.NotNull(superAdmin);
        var roles = await userManager.GetRolesAsync(superAdmin!);
        Assert.Contains(Roles.SuperAdmin, roles);
        Assert.Contains(Roles.Admin, roles);
    }

    [Fact]
    public async Task SeedAsync_CalledTwice_DoesNotDuplicateSuperAdmin()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(SeedAsync_CalledTwice_DoesNotDuplicateSuperAdmin));
        using var scope = provider.CreateScope();

        await RoleSeeder.SeedAsync(scope.ServiceProvider);
        await RoleSeeder.SeedAsync(scope.ServiceProvider);

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var count = userManager.Users.Count(u => u.Email == "superadmin@staybnb.local");

        Assert.Equal(1, count);
    }
}
