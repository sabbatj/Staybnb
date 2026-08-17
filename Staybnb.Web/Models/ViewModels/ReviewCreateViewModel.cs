using System.ComponentModel.DataAnnotations;

namespace Staybnb.Web.Models.ViewModels;

public class ReviewCreateViewModel
{
    public int PropertyId { get; set; }
    public int BookingId { get; set; }

    [Required, Range(1, 5)]
    public int Rating { get; set; } = 5;

    [MaxLength(1000)]
    public string Comment { get; set; } = string.Empty;
}
