using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Staybnb.Web.Models;

namespace Staybnb.Web.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<HostProperty> HostProperties { get; set; } = null!;
    public DbSet<PropertyImage> PropertyImages { get; set; } = null!;
    public DbSet<Amenity> Amenities { get; set; } = null!;
    public DbSet<Booking> Bookings { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<Review> Reviews { get; set; } = null!;
    public DbSet<WishlistItem> WishlistItems { get; set; } = null!;
    public DbSet<Message> Messages { get; set; } = null!;
    public DbSet<ActivityLog> ActivityLogs { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<HostApplication> HostApplications { get; set; } = null!;
    public DbSet<CheckInProcess> CheckInProcesses { get; set; } = null!;
    public DbSet<GuestCheckIn> GuestCheckIns { get; set; } = null!;
    public DbSet<GuestDocument> GuestDocuments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ---- HostProperty ----
        builder.Entity<HostProperty>()
            .HasOne(p => p.Host)
            .WithMany()
            .HasForeignKey(p => p.HostId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<HostProperty>()
            .HasOne(p => p.CheckInProcess)
            .WithOne(c => c.Property)
            .HasForeignKey<CheckInProcess>(c => c.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Many-to-many: HostProperty <-> Amenity
        builder.Entity<HostProperty>()
            .HasMany(p => p.Amenities)
            .WithMany(a => a.Properties)
            .UsingEntity(j => j.ToTable("HostPropertyAmenities"));

        // ---- PropertyImage ----
        builder.Entity<PropertyImage>()
            .HasOne(pi => pi.Property)
            .WithMany(p => p.Images)
            .HasForeignKey(pi => pi.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        // ---- Booking ----
        builder.Entity<Booking>()
            .HasOne(b => b.Property)
            .WithMany(p => p.Bookings)
            .HasForeignKey(b => b.PropertyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Booking>()
            .HasOne(b => b.Guest)
            .WithMany()
            .HasForeignKey(b => b.GuestId)
            .OnDelete(DeleteBehavior.Restrict);

        // ---- Payment (1-1 with Booking) ----
        builder.Entity<Payment>()
            .HasOne(pay => pay.Booking)
            .WithOne(b => b.Payment)
            .HasForeignKey<Payment>(pay => pay.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        // ---- Review ----
        builder.Entity<Review>()
            .HasOne(r => r.Property)
            .WithMany(p => p.Reviews)
            .HasForeignKey(r => r.PropertyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Review>()
            .HasOne(r => r.Reviewer)
            .WithMany()
            .HasForeignKey(r => r.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);

        // ---- WishlistItem ----
        builder.Entity<WishlistItem>()
            .HasOne(w => w.User)
            .WithMany()
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<WishlistItem>()
            .HasOne(w => w.Property)
            .WithMany()
            .HasForeignKey(w => w.PropertyId)
            .OnDelete(DeleteBehavior.Restrict);

        // ---- Message ----
        builder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Message>()
            .HasOne(m => m.Receiver)
            .WithMany()
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        // ---- ActivityLog ----
        builder.Entity<ActivityLog>()
            .HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ---- Notification ----
        builder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ---- HostApplication ----
        builder.Entity<HostApplication>()
            .HasOne(h => h.Applicant)
            .WithMany()
            .HasForeignKey(h => h.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<HostApplication>()
            .HasOne(h => h.Property)
            .WithMany()
            .HasForeignKey(h => h.PropertyId)
            .OnDelete(DeleteBehavior.Restrict);

        // ---- GuestCheckIn (1-1 with Booking) ----
        builder.Entity<GuestCheckIn>()
            .HasOne(g => g.Booking)
            .WithOne(b => b.GuestCheckIn)
            .HasForeignKey<GuestCheckIn>(g => g.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<GuestCheckIn>()
            .HasOne(g => g.CheckInProcess)
            .WithMany()
            .HasForeignKey(g => g.CheckInProcessId)
            .OnDelete(DeleteBehavior.Restrict);

        // ---- GuestDocument ----
        builder.Entity<GuestDocument>()
            .HasOne(d => d.GuestCheckIn)
            .WithMany(g => g.Documents)
            .HasForeignKey(d => d.GuestCheckInId)
            .OnDelete(DeleteBehavior.Cascade);

        // ---- Decimal precision safety net (in case any Column attr is missed) ----
        builder.Entity<HostProperty>().Property(p => p.PricePerNight).HasPrecision(18, 2);
        builder.Entity<HostProperty>().Property(p => p.CleaningFee).HasPrecision(18, 2);
        builder.Entity<HostProperty>().Property(p => p.ServiceFee).HasPrecision(18, 2);
        builder.Entity<Booking>().Property(b => b.TotalPrice).HasPrecision(18, 2);
        builder.Entity<Payment>().Property(p => p.Amount).HasPrecision(18, 2);
    }
}
