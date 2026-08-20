using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Staybnb.Web.Constants;
using Staybnb.Web.Data;
using Staybnb.Web.Models;

namespace Staybnb.Web.Services;

public class UserRoleService : IUserRoleService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IActivityLogService _activityLog;
    private readonly ApplicationDbContext _context;

    public UserRoleService(
        UserManager<ApplicationUser> userManager,
        IActivityLogService activityLog,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _activityLog = activityLog;
        _context = context;
    }

    public async Task<bool> PromoteGuestToHostAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        if (await _userManager.IsInRoleAsync(user, Roles.Guest))
        {
            var removeGuestResult =
                await _userManager.RemoveFromRoleAsync(user, Roles.Guest);

            if (!removeGuestResult.Succeeded)
                return false;
        }

        if (!await _userManager.IsInRoleAsync(user, Roles.Host))
        {
            var addHostResult =
                await _userManager.AddToRoleAsync(user, Roles.Host);

            if (!addHostResult.Succeeded)
                return false;
        }

        await _activityLog.LogAsync(
            userId,
            "Promoted from Guest to Host",
            ActivityType.RoleChange);

        return true;
    }

    public async Task<bool> PromoteGuestToAdminAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        if (await _userManager.IsInRoleAsync(user, Roles.Guest))
        {
            var removeGuestResult =
                await _userManager.RemoveFromRoleAsync(user, Roles.Guest);

            if (!removeGuestResult.Succeeded)
                return false;
        }

        if (!await _userManager.IsInRoleAsync(user, Roles.Admin))
        {
            var addAdminResult =
                await _userManager.AddToRoleAsync(user, Roles.Admin);

            if (!addAdminResult.Succeeded)
                return false;
        }

        await _activityLog.LogAsync(
            userId,
            "Promoted to Admin by SuperAdmin",
            ActivityType.RoleChange);

        return true;
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return false;

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            // Delete notifications belonging to the user.
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .ToListAsync();

            _context.Notifications.RemoveRange(notifications);

            // Delete activity logs belonging to the user.
            var activityLogs = await _context.ActivityLogs
                .Where(a => a.UserId == userId)
                .ToListAsync();

            _context.ActivityLogs.RemoveRange(activityLogs);

            // Delete wishlist items belonging to the user.
            var wishlistItems = await _context.WishlistItems
                .Where(w => w.UserId == userId)
                .ToListAsync();

            _context.WishlistItems.RemoveRange(wishlistItems);

            // Delete messages sent or received by the user.
            var messages = await _context.Messages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .ToListAsync();

            _context.Messages.RemoveRange(messages);

            // Delete reviews created by the user.
            var reviews = await _context.Reviews
                .Where(r => r.ReviewerId == userId)
                .ToListAsync();

            _context.Reviews.RemoveRange(reviews);

            // Delete host applications belonging to the user.
            var applications = await _context.HostApplications
                .Where(a => a.ApplicationUserId == userId)
                .ToListAsync();

            _context.HostApplications.RemoveRange(applications);

            // Find properties owned by this user.
            var properties = await _context.HostProperties
                .Include(p => p.Amenities)
                .Where(p => p.HostId == userId)
                .ToListAsync();

            var propertyIds = properties
                .Select(p => p.Id)
                .ToList();

            if (propertyIds.Count > 0)
            {
                // Delete guest check-in documents/process data
                // through their dependent booking relationships.

                var bookings = await _context.Bookings
                    .Where(b => propertyIds.Contains(b.PropertyId))
                    .ToListAsync();

                var bookingIds = bookings
                    .Select(b => b.Id)
                    .ToList();

                if (bookingIds.Count > 0)
                {
                    var guestDocuments = await _context.GuestDocuments
                        .Where(d => _context.GuestCheckIns
                            .Any(g => g.Id == d.GuestCheckInId &&
                                      bookingIds.Contains(g.BookingId)))
                        .ToListAsync();

                    _context.GuestDocuments.RemoveRange(guestDocuments);

                    var guestCheckIns = await _context.GuestCheckIns
                        .Where(g => bookingIds.Contains(g.BookingId))
                        .ToListAsync();

                    _context.GuestCheckIns.RemoveRange(guestCheckIns);

                    var payments = await _context.Payments
                        .Where(p => bookingIds.Contains(p.BookingId))
                        .ToListAsync();

                    _context.Payments.RemoveRange(payments);
                }

                // Delete bookings belonging to host properties.
                _context.Bookings.RemoveRange(bookings);

                // Delete reviews belonging to host properties.
                var propertyReviews = await _context.Reviews
                    .Where(r => propertyIds.Contains(r.PropertyId))
                    .ToListAsync();

                _context.Reviews.RemoveRange(propertyReviews);

                // Delete property images.
                var images = await _context.PropertyImages
                    .Where(i => propertyIds.Contains(i.PropertyId))
                    .ToListAsync();

                _context.PropertyImages.RemoveRange(images);

                // Delete check-in processes belonging to properties.
                var checkInProcesses = await _context.CheckInProcesses
                    .Where(c => propertyIds.Contains(c.PropertyId))
                    .ToListAsync();

                _context.CheckInProcesses.RemoveRange(checkInProcesses);

                // Clear many-to-many Property/Amenity relationships.
                foreach (var property in properties)
                {
                    property.Amenities.Clear();
                }

                // Finally delete the properties.
                _context.HostProperties.RemoveRange(properties);
            }

            // Delete the Identity user.
            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    result.Errors.Select(e => $"{e.Code}: {e.Description}"));

                Console.WriteLine($"DELETE USER FAILED: {errors}");

                await transaction.RollbackAsync();
                return false;
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DELETE USER EXCEPTION: {ex.Message}");

            await transaction.RollbackAsync();

            return false;
        }
    }

    public async Task<bool> RemoveHostAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return false;

        if (!await _userManager.IsInRoleAsync(user, Roles.Host))
            return false;

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            // Deactivate every property owned by this host.
            var properties = await _context.HostProperties
                .Where(p => p.HostId == userId && p.IsActive)
                .ToListAsync();

            foreach (var property in properties)
            {
                property.IsActive = false;
            }

            await _context.SaveChangesAsync();

            // Remove Host role.
            var removeHostResult =
                await _userManager.RemoveFromRoleAsync(user, Roles.Host);

            if (!removeHostResult.Succeeded)
            {
                await transaction.RollbackAsync();
                return false;
            }

            // A normal former host should return to Guest access.
            // Do not add Guest if this person is also an Admin.
            var isAdmin =
                await _userManager.IsInRoleAsync(user, Roles.Admin);

            var isGuest =
                await _userManager.IsInRoleAsync(user, Roles.Guest);

            if (!isAdmin && !isGuest)
            {
                var addGuestResult =
                    await _userManager.AddToRoleAsync(user, Roles.Guest);

                if (!addGuestResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            }

            _context.Notifications.Add(new Notification
            {
                UserId = userId,
                Title = "Host Access Removed",
                Message =
                    "Your Host status has been removed by an administrator. " +
                    "Your properties are no longer active.",
                Type = NotificationType.HostApplicationUpdate,
                ActionUrl = "/Notifications"
            });

            await _context.SaveChangesAsync();

            await _activityLog.LogAsync(
                userId,
                "Host access removed by Admin",
                ActivityType.RoleChange);

            await transaction.CommitAsync();

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            return false;
        }
    }
}
