using Staybnb.Web.Services;
using Xunit;

namespace Staybnb.Tests;

public class PricingCalculatorTests
{
    [Theory]
    [InlineData(1000, 3, 150, 100, 3250)]
    [InlineData(500, 1, 0, 50, 550)]
    [InlineData(2000, 5, 300, 200, 10500)]
    public void CalculateTotalPrice_MatchesBriefFormula(decimal pricePerNight, int nights, decimal cleaningFee, decimal serviceFee, decimal expected)
    {
        var result = PricingCalculator.CalculateTotalPrice(pricePerNight, nights, cleaningFee, serviceFee);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void CalculateTotalPrice_ZeroOrNegativeNights_ReturnsZero()
    {
        var result = PricingCalculator.CalculateTotalPrice(1000, 0, 150, 100);
        Assert.Equal(0, result);
    }
}
