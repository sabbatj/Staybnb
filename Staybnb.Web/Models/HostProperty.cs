using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Staybnb.Web.Models;

public class HostProperty
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal PricePerNight { get; set; }

    [Required]
    public string HostId { get; set; } = string.Empty;
    public ApplicationUser? Host { get; set; }

    [Required]
    public string Address { get; set; } = string.Empty;

    public string? City { get; set; }
    public string? PropertyType { get; set; }
    public bool IsActive { get; set; } = true;

    public int MaxGuests { get; set; }
    public int Bedrooms { get; set; }
    public int Beds { get; set; }
    public int Bathrooms { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal CleaningFee { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ServiceFee { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Amenity> Amenities { get; set; } = new List<Amenity>();
    public CheckInProcess? CheckInProcess { get; set; }
}
