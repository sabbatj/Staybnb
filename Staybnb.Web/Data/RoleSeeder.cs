using Microsoft.AspNetCore.Identity;
using Staybnb.Web.Constants;
using Staybnb.Web.Models;

namespace Staybnb.Web.Data;

public static class RoleSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var config = services.GetRequiredService<IConfiguration>();

        // 1. Create all four roles if they don't exist
        foreach (var roleName in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // 2. Seed the SuperAdmin account
        var superAdminEmail = config["SuperAdminSeed:Email"] ?? "superadmin@staybnb.local";
        var superAdminPassword = config["SuperAdminSeed:Password"] ?? "SuperAdmin123!";

        var existingSuperAdmin = await userManager.FindByEmailAsync(superAdminEmail);
        if (existingSuperAdmin == null)
        {
            var superAdmin = new ApplicationUser
            {
                UserName = superAdminEmail,
                Email = superAdminEmail,
                FirstName = "Super",
                LastName = "Admin",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(superAdmin, superAdminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(superAdmin, Roles.SuperAdmin);
                await userManager.AddToRoleAsync(superAdmin, Roles.Admin);
            }
        }
    }
}
