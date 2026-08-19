using Staybnb.Web.Constants;
using Staybnb.Web.Models;
using Xunit;

namespace Staybnb.Web.Tests;

public class Requirement1IdentityTests
{
    [Fact]
    public void RequiredRoles_AreDefined()
    {
        Assert.Equal("Guest", Roles.Guest);
        Assert.Equal("Host", Roles.Host);
        Assert.Equal("Admin", Roles.Admin);
        Assert.Equal("SuperAdmin", Roles.SuperAdmin);
    }

    [Fact]
    public void HostApplication_DefaultsToPending()
    {
        var application = new HostApplication();
        Assert.Equal(ApplicationStatus.Pending, application.Status);
    }

    [Fact]
    public void RequiredActivityTypes_Exist()
    {
        Assert.Equal(ActivityType.Login, ActivityType.Login);
        Assert.Equal(ActivityType.RoleChange, ActivityType.RoleChange);
        Assert.Equal(ActivityType.BookingStatusUpdate, ActivityType.BookingStatusUpdate);
    }
}
