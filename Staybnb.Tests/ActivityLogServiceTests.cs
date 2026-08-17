using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Staybnb.Web.Data;
using Staybnb.Web.Models;
using Staybnb.Web.Services;
using Xunit;

namespace Staybnb.Tests;

public class ActivityLogServiceTests
{
    [Fact]
    public async Task LogAsync_CreatesActivityLogEntry_WithCorrectFields()
    {
        using var provider = TestServiceProviderFactory.Create(nameof(LogAsync_CreatesActivityLogEntry_WithCorrectFields));
        using var scope = provider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var service = scope.ServiceProvider.GetRequiredService<IActivityLogService>();

        await service.LogAsync("user-123", "User logged in", ActivityType.Login);

        var log = await context.ActivityLogs.FirstOrDefaultAsync(l => l.UserId == "user-123");

        Assert.NotNull(log);
        Assert.Equal("User logged in", log!.Action);
        Assert.Equal(ActivityType.Login, log.ActivityType);
    }
}
