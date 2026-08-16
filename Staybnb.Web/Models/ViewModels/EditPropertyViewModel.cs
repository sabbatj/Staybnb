using System.ComponentModel.DataAnnotations;

namespace Staybnb.Web.Models.ViewModels;

public class EditPropertyViewModel
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required, Range(1, 1000000)]
    public decimal PricePerNight { get; set; }

    [Required]
    public string Address { get; set; } = string.Empty;

    public string? City { get; set; }
    public string? PropertyType { get; set; }

    public int MaxGuests { get; set; }
    public int Bedrooms { get; set; }
    public int Beds { get; set; }
    public int Bathrooms { get; set; }

    public decimal CleaningFee { get; set; }
    public decimal ServiceFee { get; set; }
}
