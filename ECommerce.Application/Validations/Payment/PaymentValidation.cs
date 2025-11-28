using ECommerce.Domain.Model;
using FluentValidation;

namespace ECommerce.Application.Validations.Payment;

public class PaymentValidation : AbstractValidator<PaymentCard>
{
    public PaymentValidation()
    {
        RuleFor(x => x.CardHolderName).NotEmpty().MinimumLength(2).MaximumLength(50).Matches(@"^[A-Za-z\s]+$");

        RuleFor(x => x.CardNumber).NotEmpty().CreditCard();

        RuleFor(x => x.ExpirationMonth).NotEmpty().InclusiveBetween(1, 12);

        RuleFor(x => new { x.ExpirationMonth, x.ExpirationYear }).NotEmpty().Must(x => IsValidExpirationDate(x.ExpirationMonth, x.ExpirationYear));

        RuleFor(x => x.Cvc).NotEmpty().Matches(@"^\d{3,4}$");
    }

    private bool IsValidExpirationDate(int month, int year)
    {
        const int maxYearsInFuture = 20;

        var today = DateTime.UtcNow.Date;
        var currentMonth = today.Month;
        var currentYear = today.Year;

        if (year < 100)
        {
            year += 2000;
        }

        if (year > currentYear + maxYearsInFuture)
        {
            return false;
        }

        if (year < currentYear)
        {
            return false;
        }

        if (year == currentYear && month < currentMonth)
        {
            return false;
        }

        return true;
    }
}
