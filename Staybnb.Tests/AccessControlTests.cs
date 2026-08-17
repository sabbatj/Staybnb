using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Staybnb.Web.Constants;
using Staybnb.Web.Controllers;
using Xunit;

namespace Staybnb.Tests;

public class AccessControlTests
{
    [Fact]
    public void HostController_RequiresHostRole()
    {
        var attr = typeof(HostController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(attr);
        Assert.Equal(Roles.Host, attr!.Roles);
    }

    [Fact]
    public void BookingController_RequiresGuestRole()
    {
        var attr = typeof(BookingController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(attr);
        Assert.Equal(Roles.Guest, attr!.Roles);
    }

    [Fact]
    public void AdminController_RequiresAdminRole()
    {
        var attr = typeof(AdminController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(attr);
        Assert.Equal(Roles.Admin, attr!.Roles);
    }

    [Fact]
    public void AdminController_UsersAction_RequiresSuperAdminRole()
    {
        var method = typeof(AdminController).GetMethod("Users");
        var attr = method!.GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(attr);
        Assert.Equal(Roles.SuperAdmin, attr!.Roles);
    }

    [Fact]
    public void AdminController_PromoteToAdminAction_RequiresSuperAdminRole()
    {
        var method = typeof(AdminController).GetMethod("PromoteToAdmin");
        var attr = method!.GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(attr);
        Assert.Equal(Roles.SuperAdmin, attr!.Roles);
    }

    [Fact]
    public void HostingController_BecomeHostActions_RequireGuestRole()
    {
        var methods = typeof(HostingController).GetMethods().Where(m => m.Name == "BecomeHost");
        Assert.NotEmpty(methods);
        Assert.All(methods, m =>
        {
            var attr = m.GetCustomAttribute<AuthorizeAttribute>();
            Assert.NotNull(attr);
            Assert.Equal(Roles.Guest, attr!.Roles);
        });
    }
}
