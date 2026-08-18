using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Staybnb.Web.Constants;
using Staybnb.Web.Data;
using Staybnb.Web.Models;
using Xunit;

namespace Staybnb.Tests;

public class HostApplicationFlowTests
{
    [Fact]
    public async Task GuestCanCreatePendingHostApplication()
    {
        using var provider =
            TestServiceProviderFactory.Create(nameof(GuestCanCreatePendingHostApplication));

        using var scope = provider.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser
        {
            UserName = "application.guest@test.local",
            Email = "application.guest@test.local",
            FirstName = "Application",
            LastName = "Guest",
            CreatedAt = DateTime.UtcNow
        };

        var createResult =
            await userManager.CreateAsync(user, "TestPassword123!");

        Assert.True(createResult.Succeeded);

        await userManager.AddToRoleAsync(user, Roles.Guest);

        var property = new HostProperty
        {
            Title = "Flow Test Property",
            Description = "Property used for automated flow testing.",
            HostId = user.Id,
            Address = "1 Test Street",
            City = "Cape Town",
            PropertyType = "Apartment",
            PricePerNight = 1500,
            MaxGuests = 2,
            Bedrooms = 1,
            Beds = 1,
            Bathrooms = 1,
            CleaningFee = 100,
            ServiceFee = 50,
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };

        context.HostProperties.Add(property);
        await context.SaveChangesAsync();

        var application = new HostApplication
        {
            ApplicationUserId = user.Id,
            PropertyId = property.Id,
            Status = ApplicationStatus.Pending,
            AppliedAt = DateTime.UtcNow
        };

        context.HostApplications.Add(application);
        await context.SaveChangesAsync();

        var savedApplication = await context.HostApplications
            .Include(a => a.Property)
            .FirstOrDefaultAsync(a => a.Id == application.Id);

        Assert.NotNull(savedApplication);
        Assert.Equal(ApplicationStatus.Pending, savedApplication!.Status);
        Assert.Equal(user.Id, savedApplication.ApplicationUserId);
        Assert.Equal(property.Id, savedApplication.PropertyId);
        Assert.NotNull(savedApplication.Property);
        Assert.False(savedApplication.Property!.IsActive);
    }

    [Fact]
    public async Task ApprovedApplicationCanActivateProperty()
    {
        using var provider =
            TestServiceProviderFactory.Create(nameof(ApprovedApplicationCanActivateProperty));

        using var scope = provider.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser
        {
            UserName = "approval.guest@test.local",
            Email = "approval.guest@test.local",
            FirstName = "Approval",
            LastName = "Guest",
            CreatedAt = DateTime.UtcNow
        };

        var createResult =
            await userManager.CreateAsync(user, "TestPassword123!");

        Assert.True(createResult.Succeeded);

        await userManager.AddToRoleAsync(user, Roles.Guest);

        var property = new HostProperty
        {
            Title = "Approval Test Property",
            Description = "Approval flow test.",
            HostId = user.Id,
            Address = "2 Test Street",
            City = "Cape Town",
            PropertyType = "Cabin",
            PricePerNight = 1200,
            MaxGuests = 4,
            Bedrooms = 2,
            Beds = 2,
            Bathrooms = 1,
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };

        context.HostProperties.Add(property);
        await context.SaveChangesAsync();

        var application = new HostApplication
        {
            ApplicationUserId = user.Id,
            PropertyId = property.Id,
            Status = ApplicationStatus.Pending,
            AppliedAt = DateTime.UtcNow
        };

        context.HostApplications.Add(application);
        await context.SaveChangesAsync();

        application.Status = ApplicationStatus.Approved;
        property.IsActive = true;

        await context.SaveChangesAsync();

        var savedProperty = await context.HostProperties
            .FirstAsync(p => p.Id == property.Id);

        var savedApplication = await context.HostApplications
            .FirstAsync(a => a.Id == application.Id);

        Assert.Equal(ApplicationStatus.Approved, savedApplication.Status);
        Assert.True(savedProperty.IsActive);
    }

    [Fact]
    public async Task AdminNotificationIsStoredForPendingHostApplication()
    {
        using var provider =
            TestServiceProviderFactory.Create(nameof(AdminNotificationIsStoredForPendingHostApplication));

        using var scope = provider.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        var admin = new ApplicationUser
        {
            UserName = "notification.admin@test.local",
            Email = "notification.admin@test.local",
            FirstName = "Notification",
            LastName = "Admin",
            CreatedAt = DateTime.UtcNow
        };

        var adminResult =
            await userManager.CreateAsync(admin, "TestPassword123!");

        Assert.True(adminResult.Succeeded);

        await userManager.AddToRoleAsync(admin, Roles.Admin);

        var guest = new ApplicationUser
        {
            UserName = "notification.guest@test.local",
            Email = "notification.guest@test.local",
            FirstName = "Notification",
            LastName = "Guest",
            CreatedAt = DateTime.UtcNow
        };

        var guestResult =
            await userManager.CreateAsync(guest, "TestPassword123!");

        Assert.True(guestResult.Succeeded);

        await userManager.AddToRoleAsync(guest, Roles.Guest);

        var property = new HostProperty
        {
            Title = "Notification Flow Property",
            Description = "Notification test property.",
            HostId = guest.Id,
            Address = "3 Test Street",
            City = "Cape Town",
            PropertyType = "Apartment",
            PricePerNight = 1000,
            MaxGuests = 2,
            Bedrooms = 1,
            Beds = 1,
            Bathrooms = 1,
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };

        context.HostProperties.Add(property);
        await context.SaveChangesAsync();

        var application = new HostApplication
        {
            ApplicationUserId = guest.Id,
            PropertyId = property.Id,
            Status = ApplicationStatus.Pending,
            AppliedAt = DateTime.UtcNow
        };

        context.HostApplications.Add(application);
        await context.SaveChangesAsync();

        context.Notifications.Add(new Notification
        {
            UserId = admin.Id,
            Title = "New Host Application",
            Message = $"{guest.Email} submitted a host application for '{property.Title}'.",
            Type = NotificationType.HostApplicationUpdate,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        var notification = await context.Notifications
            .FirstOrDefaultAsync(n =>
                n.UserId == admin.Id &&
                n.Title == "New Host Application");

        Assert.NotNull(notification);
        Assert.Equal(admin.Id, notification!.UserId);
        Assert.Contains(property.Title, notification.Message);
        Assert.False(notification.IsRead);
    }
}
