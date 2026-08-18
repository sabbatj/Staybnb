using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Staybnb.Web.Constants;
using Staybnb.Web.Models;
using Staybnb.Web.Models.ViewModels;
using Xunit;

namespace Staybnb.Tests;

public class RegistrationFlowTests
{
    [Fact]
    public async Task Registration_CreatesUserWithGuestRole()
    {
        var provider = TestServiceProviderFactory.Create(
            nameof(Registration_CreatesUserWithGuestRole));

        using var scope = provider.CreateScope();

        var userManager =
            scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var roleManager =
            scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        if (!await roleManager.RoleExistsAsync(Roles.Guest))
        {
            await roleManager.CreateAsync(new IdentityRole(Roles.Guest));
        }

        var user = new ApplicationUser
        {
            UserName = "registration@test.local",
            Email = "registration@test.local",
            FirstName = "Registration",
            LastName = "Test",
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(
            user,
            "TestPassword123!");

        Assert.True(result.Succeeded);

        await userManager.AddToRoleAsync(user, Roles.Guest);

        var stored = await userManager.FindByEmailAsync(
            "registration@test.local");

        Assert.NotNull(stored);

        var roles = await userManager.GetRolesAsync(stored);

        Assert.Contains(Roles.Guest, roles);
        Assert.DoesNotContain(Roles.Host, roles);
    }
}
