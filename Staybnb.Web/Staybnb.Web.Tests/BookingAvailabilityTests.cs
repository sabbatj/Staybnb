using Xunit;

namespace Staybnb.Web.Tests;

public class BookingAvailabilityTests
{
    private static bool Overlaps(
        DateTime requestedCheckIn,
        DateTime requestedCheckOut,
        DateTime existingCheckIn,
        DateTime existingCheckOut)
    {
        return requestedCheckIn < existingCheckOut &&
               requestedCheckOut > existingCheckIn;
    }

    [Fact]
    public void Rejects_Overlapping_Booking()
    {
        var existingIn = new DateTime(2026, 8, 19);
        var existingOut = new DateTime(2026, 8, 20);

        var requestedIn = new DateTime(2026, 8, 19);
        var requestedOut = new DateTime(2026, 8, 20);

        Assert.True(Overlaps(
            requestedIn,
            requestedOut,
            existingIn,
            existingOut));
    }

    [Fact]
    public void Allows_Booking_After_Existing_Checkout()
    {
        var existingIn = new DateTime(2026, 8, 19);
        var existingOut = new DateTime(2026, 8, 20);

        var requestedIn = new DateTime(2026, 8, 20);
        var requestedOut = new DateTime(2026, 8, 21);

        Assert.False(Overlaps(
            requestedIn,
            requestedOut,
            existingIn,
            existingOut));
    }

    [Fact]
    public void Allows_Booking_Before_Existing_Checkin()
    {
        var existingIn = new DateTime(2026, 8, 19);
        var existingOut = new DateTime(2026, 8, 20);

        var requestedIn = new DateTime(2026, 8, 18);
        var requestedOut = new DateTime(2026, 8, 19);

        Assert.False(Overlaps(
            requestedIn,
            requestedOut,
            existingIn,
            existingOut));
    }
}
