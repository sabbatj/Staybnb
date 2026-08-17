using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Staybnb.Web.Data;
using Staybnb.Web.Models;
using Staybnb.Web.Services;

namespace Staybnb.Tests;

public static class TestServiceProviderFactory
{
    public static ServiceProvider Create(string dbName)
    {
        var services = new ServiceCollection();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(dbName));

        services.AddLogging();

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 4;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        var configValues = new Dictionary<string, string?>
        {
            { "SuperAdminSeed:Email", "superadmin@staybnb.local" },
            { "SuperAdminSeed:Password", "SuperAdmin123!" }
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(configValues).Build();
        services.AddSingleton<IConfiguration>(config);

        services.AddScoped<IActivityLogService, ActivityLogService>();
        services.AddScoped<IUserRoleService, UserRoleService>();

        return services.BuildServiceProvider();
    }
}
