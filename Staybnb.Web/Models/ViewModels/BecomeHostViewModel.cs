using System.ComponentModel.DataAnnotations;

namespace Staybnb.Web.Models.ViewModels;

public class BecomeHostViewModel
{
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

    public int MaxGuests { get; set; } = 1;
    public int Bedrooms { get; set; } = 1;
    public int Beds { get; set; } = 1;
    public int Bathrooms { get; set; } = 1;

    public decimal CleaningFee { get; set; }
    public decimal ServiceFee { get; set; }

    [Required(ErrorMessage = "At least one property image is required.")]
    public List<IFormFile> Images { get; set; } = new();
}
