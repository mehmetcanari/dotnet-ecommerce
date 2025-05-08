namespace ECommerce.Application.Utility;

public static class MathService
{
    public static decimal CalculateDiscount(decimal price, decimal discountRate)
    {
        try
        {
            if (discountRate < 0 || discountRate > 100)
                throw new ArgumentException("Discount rate must be between 0 and 100");

            return price - (price * discountRate / 100);
        }
        catch (Exception ex)
        {
            throw new Exception("Error calculating discount", ex);
        }
    }
}