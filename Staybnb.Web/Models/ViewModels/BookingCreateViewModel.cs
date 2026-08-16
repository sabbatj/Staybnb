using System.ComponentModel.DataAnnotations;

namespace Staybnb.Web.Models.ViewModels;

public class BookingCreateViewModel
{
    public int PropertyId { get; set; }

    [Required, DataType(DataType.Date)]
    public DateTime CheckInDate { get; set; } = DateTime.Today.AddDays(1);

    [Required, DataType(DataType.Date)]
    public DateTime CheckOutDate { get; set; } = DateTime.Today.AddDays(2);

    [Required, Range(1, 50)]
    public int NumberOfGuests { get; set; } = 1;
}
