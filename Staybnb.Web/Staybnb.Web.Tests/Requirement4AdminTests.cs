using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Staybnb.Web.Constants;
using Staybnb.Web.Data;
using Staybnb.Web.Models;
using Staybnb.Web.Services;

namespace Staybnb.Web.Tests;

public class Requirement4AdminTests
{
    private static (ApplicationDbContext Context, UserManager<ApplicationUser> UserManager, RoleManager<IdentityRole> RoleManager, UserRoleService Service)
        CreateSystem()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(
                Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var context = new ApplicationDbContext(options);

        var userStore = new UserStore<ApplicationUser, IdentityRole, ApplicationDbContext, string>(context);
        var roleStore = new RoleStore<IdentityRole, ApplicationDbContext, string>(context);

        var userManager = new UserManager<ApplicationUser>(
            userStore,
            null!,
            new PasswordHasher<ApplicationUser>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null!,
            null!);

        var roleManager = new RoleManager<IdentityRole>(
            roleStore,
            Array.Empty<IRoleValidator<IdentityRole>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null!);

        var activityLog = new ActivityLogService(context);
        var service = new UserRoleService(userManager, activityLog, context);

        return (context, userManager, roleManager, service);
    }

    private static async Task<ApplicationUser> CreateUser(
        UserManager<ApplicationUser> userManager,
        string email)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = "Test",
            LastName = "User"
        };

        var result = await userManager.CreateAsync(user, "Test123!");
        Assert.True(result.Succeeded);

        return user;
    }

    private static async Task EnsureRoles(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(role));
                Assert.True(result.Succeeded);
            }
        }
    }

    [Fact]
    public async Task Guest_Can_Be_Promoted_To_Host_And_Guest_Is_Removed()
    {
        var system = CreateSystem();
        await EnsureRoles(system.RoleManager);

        var user = await CreateUser(system.UserManager, "guest-host@test.com");
        await system.UserManager.AddToRoleAsync(user, Roles.Guest);

        var result = await system.Service.PromoteGuestToHostAsync(user.Id);

        Assert.True(result);
        Assert.True(await system.UserManager.IsInRoleAsync(user, Roles.Host));
        Assert.False(await system.UserManager.IsInRoleAsync(user, Roles.Guest));
    }

    [Fact]
    public async Task Guest_Can_Be_Promoted_To_Admin_And_Guest_Is_Removed()
    {
        var system = CreateSystem();
        await EnsureRoles(system.RoleManager);

        var user = await CreateUser(system.UserManager, "guest-admin@test.com");
        await system.UserManager.AddToRoleAsync(user, Roles.Guest);

        var result = await system.Service.PromoteGuestToAdminAsync(user.Id);

        Assert.True(result);
        Assert.True(await system.UserManager.IsInRoleAsync(user, Roles.Admin));
        Assert.False(await system.UserManager.IsInRoleAsync(user, Roles.Guest));
    }

    [Fact]
    public async Task Guest_To_Admin_Promotion_Creates_RoleChange_Audit_Log()
    {
        var system = CreateSystem();
        await EnsureRoles(system.RoleManager);

        var user = await CreateUser(system.UserManager, "audit-admin@test.com");
        await system.UserManager.AddToRoleAsync(user, Roles.Guest);

        var result = await system.Service.PromoteGuestToAdminAsync(user.Id);

        Assert.True(result);

        var log = await system.Context.ActivityLogs
            .Where(x => x.UserId == user.Id)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();

        Assert.NotNull(log);
        Assert.Equal(ActivityType.RoleChange, log.ActivityType);
        Assert.Contains("Promoted to Admin", log.Action);
    }

    [Fact]
    public async Task Host_Can_Be_Removed_And_Returns_To_Guest()
    {
        var system = CreateSystem();
        await EnsureRoles(system.RoleManager);

        var user = await CreateUser(system.UserManager, "host-remove@test.com");
        await system.UserManager.AddToRoleAsync(user, Roles.Host);

        var result = await system.Service.RemoveHostAsync(user.Id);

        Assert.True(result);
        Assert.False(await system.UserManager.IsInRoleAsync(user, Roles.Host));
        Assert.True(await system.UserManager.IsInRoleAsync(user, Roles.Guest));
    }
}
