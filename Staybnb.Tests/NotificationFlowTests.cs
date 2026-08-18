using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Staybnb.Web.Data;
using Staybnb.Web.Models;
using Xunit;

namespace Staybnb.Tests;

public class NotificationFlowTests
{
    [Fact]
    public async Task AdminNotification_IsUnreadByDefault()
    {
        var provider = TestServiceProviderFactory.Create(
            nameof(AdminNotification_IsUnreadByDefault));

        using var scope = provider.CreateScope();

        var context =
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Notifications.Add(new Notification
        {
            UserId = "admin-id",
            Title = "New Host Application",
            Message = "guest@test.local submitted a host application for 'FlowTestAre'.",
            Type = NotificationType.HostApplicationUpdate
        });

        await context.SaveChangesAsync();

        var notification = await context.Notifications
            .SingleAsync();

        Assert.False(notification.IsRead);
        Assert.Equal("New Host Application", notification.Title);
        Assert.Contains("FlowTestAre", notification.Message);
    }

    [Fact]
    public async Task Notification_BelongsOnlyToTargetAdmin()
    {
        var provider = TestServiceProviderFactory.Create(
            nameof(Notification_BelongsOnlyToTargetAdmin));

        using var scope = provider.CreateScope();

        var context =
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Notifications.Add(new Notification
        {
            UserId = "admin-1",
            Title = "New Host Application",
            Message = "Application notification",
            Type = NotificationType.HostApplicationUpdate
        });

        await context.SaveChangesAsync();

        var admin1Count = await context.Notifications
            .CountAsync(n => n.UserId == "admin-1");

        var admin2Count = await context.Notifications
            .CountAsync(n => n.UserId == "admin-2");

        Assert.Equal(1, admin1Count);
        Assert.Equal(0, admin2Count);
    }
}
