using ECommerce.Shared.Constants;

namespace ECommerce.Application.Utility;

public static class MathService
{
    public static decimal CalculateDiscount(decimal price, decimal discountRate)
    {
        try
        {
            if (discountRate < 0 || discountRate > 100)
                return 0;

            return price - price * discountRate / 100;
        }
        catch (Exception ex)
        {
            throw new Exception(ErrorMessages.ErrorCalculatingDiscountedPrice, ex);
        }
    }
}