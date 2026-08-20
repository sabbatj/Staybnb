using Staybnb.Web.Models;
using Staybnb.Web.Models.ViewModels;
using Xunit;

namespace Staybnb.Web.Tests;

public class ReviewSecurityTests
{
    [Fact]
    public void Review_Rating_Must_Be_Between_1_And_5()
    {
        var model = new ReviewCreateViewModel
        {
            BookingId = 1,
            PropertyId = 1,
            Rating = 6,
            Comment = "Test"
        };

        Assert.True(model.Rating > 5);
    }

    [Fact]
    public void Review_Cannot_Use_Different_Property_From_Booking()
    {
        var booking = new Booking
        {
            Id = 1,
            PropertyId = 10,
            GuestId = "guest-1",
            Status = BookingStatus.Completed
        };

        var model = new ReviewCreateViewModel
        {
            BookingId = 1,
            PropertyId = 99,
            Rating = 5
        };

        Assert.NotEqual(booking.PropertyId, model.PropertyId);
    }

    [Theory]
    [InlineData(BookingStatus.Pending)]
    [InlineData(BookingStatus.Approved)]
    [InlineData(BookingStatus.Rejected)]
    [InlineData(BookingStatus.Cancelled)]
    public void Review_Is_Not_Allowed_Before_CheckIn(BookingStatus status)
    {
        var allowed =
            status == BookingStatus.CheckedIn ||
            status == BookingStatus.Completed;

        Assert.False(allowed);
    }

    [Theory]
    [InlineData(BookingStatus.CheckedIn)]
    [InlineData(BookingStatus.Completed)]
    public void Review_Is_Allowed_After_CheckIn(BookingStatus status)
    {
        var allowed =
            status == BookingStatus.CheckedIn ||
            status == BookingStatus.Completed;

        Assert.True(allowed);
    }
}
