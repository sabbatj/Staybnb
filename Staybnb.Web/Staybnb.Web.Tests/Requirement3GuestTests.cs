using Staybnb.Web.Models;
using Xunit;

namespace Staybnb.Web.Tests;

public class Requirement3GuestTests
{
    [Fact]
    public void Booking_StoresGuestDatesAndGuests()
    {
        var booking = new Booking
        {
            GuestId = "guest-1",
            CheckInDate = new DateTime(2026, 8, 20),
            CheckOutDate = new DateTime(2026, 8, 23),
            NumberOfGuests = 3
        };

        Assert.Equal("guest-1", booking.GuestId);
        Assert.Equal(3, booking.NumberOfGuests);
        Assert.Equal(BookingStatus.Pending, booking.Status);
    }

    [Fact]
    public void Booking_StoresTotalPrice()
    {
        var booking = new Booking { TotalPrice = 1250.50m };

        Assert.Equal(1250.50m, booking.TotalPrice);
    }

    [Fact]
    public void GuestDocument_SupportsIdAndPassport()
    {
        var id = new GuestDocument
        {
            DocumentType = "ID",
            FileName = "id.pdf"
        };

        var passport = new GuestDocument
        {
            DocumentType = "Passport",
            FileName = "passport.pdf"
        };

        Assert.Equal("ID", id.DocumentType);
        Assert.Equal("Passport", passport.DocumentType);
        Assert.Equal(DocumentStatus.Pending, id.Status);
    }
}
