using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Staybnb.Web.Constants;
using Staybnb.Web.Data;
using Staybnb.Web.Models;
using Staybnb.Web.Services;

namespace Staybnb.Tests;

public static class TestServiceProviderFactory
{
    public static ServiceProvider Create(string dbName)
    {
        var services = new ServiceCollection();

        // Test configuration required by RoleSeeder and other services.
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Seed:SuperAdminEmail"] = "test-admin@staybnb.local",
                ["Seed:SuperAdminPassword"] = "TestPassword123!",
                ["SuperAdmin:Email"] = "test-admin@staybnb.local",
                ["SuperAdmin:Password"] = "TestPassword123!"
            })
            .Build();

        services.AddSingleton<IConfiguration>(configuration);

        services.AddLogging();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(dbName));

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager();

        // Application services used by the flow tests.
        services.AddScoped<IActivityLogService, ActivityLogService>();
        services.AddScoped<IUserRoleService, UserRoleService>();

        var provider = services.BuildServiceProvider();

        SeedRolesAsync(provider).GetAwaiter().GetResult();

        return provider;
    }

    private static async Task SeedRolesAsync(ServiceProvider provider)
    {
        using var scope = provider.CreateScope();

        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(
                    new IdentityRole(role));

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to create test role '{role}': " +
                        string.Join(
                            ", ",
                            result.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}
