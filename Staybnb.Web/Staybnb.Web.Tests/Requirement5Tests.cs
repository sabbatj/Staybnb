using Microsoft.EntityFrameworkCore;
using Staybnb.Web.Data;
using Staybnb.Web.Models;
using Staybnb.Web.Services;
using Xunit;

namespace Staybnb.Web.Tests;

public class Requirement5Tests
{
    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Notification_Can_Be_Stored_In_Database()
    {
        await using var context = CreateContext();

        context.Notifications.Add(new Notification
        {
            UserId = "user",
            Title = "New Message",
            Message = "You have a new message.",
            Type = NotificationType.NewMessage,
            IsRead = false
        });

        await context.SaveChangesAsync();

        var saved = await context.Notifications.SingleAsync();

        Assert.Equal("user", saved.UserId);
        Assert.Equal("New Message", saved.Title);
        Assert.Equal(NotificationType.NewMessage, saved.Type);
        Assert.False(saved.IsRead);
    }

    [Fact]
    public async Task ActivityLogService_Stores_Login_Activity()
    {
        await using var context = CreateContext();
        var service = new ActivityLogService(context);

        await service.LogAsync(
            "user",
            "User logged in",
            ActivityType.Login);

        var log = await context.ActivityLogs.SingleAsync();

        Assert.Equal("user", log.UserId);
        Assert.Equal("User logged in", log.Action);
        Assert.Equal(ActivityType.Login, log.ActivityType);
    }

    [Fact]
    public async Task ActivityLogService_Stores_Booking_Status_Activity()
    {
        await using var context = CreateContext();
        var service = new ActivityLogService(context);

        await service.LogAsync(
            "user",
            "Booking #1 status changed to Approved",
            ActivityType.BookingStatusUpdate);

        var log = await context.ActivityLogs.SingleAsync();

        Assert.Equal("user", log.UserId);
        Assert.Equal(ActivityType.BookingStatusUpdate, log.ActivityType);
        Assert.Contains("Approved", log.Action);
    }
}
