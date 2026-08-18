using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Staybnb.Web.Data;
using Staybnb.Web.Models;
using Xunit;

namespace Staybnb.Tests;

public class HostBookingWorkflowTests
{
    private static async Task<(ApplicationDbContext context, int propertyId, int bookingId)> SeedBookingAsync(string dbName)
    {
        var provider = TestServiceProviderFactory.Create(dbName);
        var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var property = new HostProperty
        {
            Title = "Test Cabin",
            Description = "A cabin for testing",
            PricePerNight = 1000m,
            HostId = "host-1",
            Address = "1 Test Street",
            CleaningFee = 150m,
            ServiceFee = 100m,
            IsActive = true
        };
        context.HostProperties.Add(property);
        await context.SaveChangesAsync();

        var booking = new Booking
        {
            PropertyId = property.Id,
            GuestId = "guest-1",
            CheckInDate = DateTime.Today.AddDays(1),
            CheckOutDate = DateTime.Today.AddDays(3),
            NumberOfGuests = 2,
            TotalPrice = 2250.00m, // (1000*2) + 150 + 100
            Status = BookingStatus.Pending
        };
        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        return (context, property.Id, booking.Id);
    }

    [Fact]
    public async Task Booking_TotalPrice_PersistsWithCorrectPrecision()
    {
        var (context, _, bookingId) = await SeedBookingAsync(nameof(Booking_TotalPrice_PersistsWithCorrectPrecision));

        var booking = await context.Bookings.FindAsync(bookingId);

        Assert.NotNull(booking);
        Assert.Equal(2250.00m, booking!.TotalPrice);
    }

    [Fact]
    public async Task Booking_ApprovedStatus_UpdatesCorrectly()
    {
        var (context, _, bookingId) = await SeedBookingAsync(nameof(Booking_ApprovedStatus_UpdatesCorrectly));

        var booking = await context.Bookings.FindAsync(bookingId);
        booking!.Status = BookingStatus.Approved;
        await context.SaveChangesAsync();

        var updated = await context.Bookings.FindAsync(bookingId);
        Assert.Equal(BookingStatus.Approved, updated!.Status);
    }

    [Fact]
    public async Task Booking_RejectedStatus_UpdatesCorrectly()
    {
        var (context, _, bookingId) = await SeedBookingAsync(nameof(Booking_RejectedStatus_UpdatesCorrectly));

        var booking = await context.Bookings.FindAsync(bookingId);
        booking!.Status = BookingStatus.Rejected;
        await context.SaveChangesAsync();

        var updated = await context.Bookings.FindAsync(bookingId);
        Assert.Equal(BookingStatus.Rejected, updated!.Status);
    }

    [Fact]
    public async Task Booking_ScopedToHostProperty_OnlyMatchesOwningHost()
    {
        var (context, propertyId, bookingId) = await SeedBookingAsync(nameof(Booking_ScopedToHostProperty_OnlyMatchesOwningHost));

        // Simulates the HostController's ownership check: b.Property.HostId == CurrentUserId
        var visibleToOwningHost = await context.Bookings
            .Include(b => b.Property)
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.Property!.HostId == "host-1");

        var visibleToOtherHost = await context.Bookings
            .Include(b => b.Property)
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.Property!.HostId == "some-other-host");

        Assert.NotNull(visibleToOwningHost);
        Assert.Null(visibleToOtherHost);
    }

    [Theory]
    [InlineData("Approved", BookingStatus.Approved)]
    [InlineData("Rejected", BookingStatus.Rejected)]
    [InlineData("CheckedIn", BookingStatus.CheckedIn)]
    public void BookingStatus_EnumParsing_MatchesControllerLogic(string statusString, BookingStatus expected)
    {
        // Mirrors: Enum.TryParse<BookingStatus>(status, out var newStatus) in HostController.UpdateBookingStatus
        var parsed = Enum.TryParse<BookingStatus>(statusString, out var result);

        Assert.True(parsed);
        Assert.Equal(expected, result);
    }
}
