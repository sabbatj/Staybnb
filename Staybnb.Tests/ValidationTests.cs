using System.ComponentModel.DataAnnotations;
using Staybnb.Web.Models.ViewModels;
using Xunit;

namespace Staybnb.Tests;

public class ValidationTests
{
    private static IList<ValidationResult> Validate(object model)
    {
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void BecomeHostViewModel_MissingRequiredFields_FailsValidation()
    {
        var model = new BecomeHostViewModel();
        var results = Validate(model);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(BecomeHostViewModel.Title)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(BecomeHostViewModel.Address)));
    }

    [Fact]
    public void BookingCreateViewModel_NumberOfGuestsOutOfRange_FailsValidation()
    {
        var model = new BookingCreateViewModel { NumberOfGuests = 0 };
        var results = Validate(model);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(BookingCreateViewModel.NumberOfGuests)));
    }

    [Fact]
    public void BookingCreateViewModel_ValidGuestCount_PassesValidation()
    {
        var model = new BookingCreateViewModel
        {
            NumberOfGuests = 2,
            CheckInDate = DateTime.Today.AddDays(1),
            CheckOutDate = DateTime.Today.AddDays(3)
        };
        var results = Validate(model);

        Assert.DoesNotContain(results, r => r.MemberNames.Contains(nameof(BookingCreateViewModel.NumberOfGuests)));
    }
}
