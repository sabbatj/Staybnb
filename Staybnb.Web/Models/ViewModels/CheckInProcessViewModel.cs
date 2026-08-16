using System.ComponentModel.DataAnnotations;

namespace Staybnb.Web.Models.ViewModels;

public class CheckInProcessViewModel
{
    public int PropertyId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = "Check-in Instructions";

    [Required]
    public string StepsText { get; set; } = string.Empty; // one step per line
}
