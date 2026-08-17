namespace Staybnb.Web.Services;

public static class PricingCalculator
{
    // TotalPrice = (PricePerNight x Nights) + CleaningFee + ServiceFee
    public static decimal CalculateTotalPrice(decimal pricePerNight, int nights, decimal cleaningFee, decimal serviceFee)
    {
        if (nights <= 0) return 0;
        return (pricePerNight * nights) + cleaningFee + serviceFee;
    }
}
