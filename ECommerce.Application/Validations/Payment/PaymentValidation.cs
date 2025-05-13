using ECommerce.Domain.Model;
using FluentValidation;

namespace ECommerce.Application.Validations.Payment;

public class PaymentValidation : AbstractValidator<PaymentCard>
{
    public PaymentValidation()
    {
        RuleFor(x => x.CardHolderName)
            .NotEmpty()
            .WithMessage("Card holder name is required.")
            .MinimumLength(2)
            .WithMessage("Card holder name must be at least 2 characters.")
            .MaximumLength(50)
            .WithMessage("Card holder name must be at most 50 characters.")
            .Matches(@"^[A-Za-z\s]+$")
            .WithMessage("Card holder name must contain only letters and spaces.");

        RuleFor(x => x.CardNumber)
            .NotEmpty()
            .WithMessage("Card number is required.")
            .CreditCard()
            .WithMessage("Card number is not valid.");

        RuleFor(x => x.ExpirationMonth)
            .NotEmpty()
            .WithMessage("Expiration month is required.")
            .InclusiveBetween(1, 12)
            .WithMessage("Expiration month must be between 1 and 12.");

        RuleFor(x => new { x.ExpirationMonth, x.ExpirationYear })
            .NotEmpty()
            .WithMessage("Expiration date is required.")
            .Must(x => IsValidExpirationDate(x.ExpirationMonth, x.ExpirationYear))
            .WithMessage("Card has expired or expiration date is invalid.");

        RuleFor(x => x.CVC)
            .NotEmpty()
            .WithMessage("CVV is required.")
            .Matches(@"^\d{3,4}$")
            .WithMessage("CVV must be 3 or 4 digits.");
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
