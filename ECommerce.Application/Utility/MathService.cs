using ECommerce.Shared.Constants;

namespace ECommerce.Application.Utility;

public static class MathService
{
    public static double CalculateDiscount(double price, double discountRate)
    {
        try
        {
            if (discountRate < 0 || discountRate > 100)
                throw new ArgumentException(ErrorMessages.InvalidDiscountRate);

            return price - price * discountRate / 100;
        }
        catch (Exception ex)
        {
            throw new Exception(ErrorMessages.ErrorCalculatingDiscountedPrice, ex);
        }
    }
}