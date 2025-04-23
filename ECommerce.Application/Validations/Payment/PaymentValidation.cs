using ECommerce.Domain.Model;
using FluentValidation;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ECommerce.Application.Validations.Payment;

public class PaymentValidation : AbstractValidator<PaymentDetails>
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
            .WithMessage("Card number is not valid.")
            .Must(IsValidCardNumber)
            .WithMessage("Invalid card number format.");

        RuleFor(x => x.ExpirationDate)
            .NotEmpty()
            .WithMessage("Expiration date is required.")
            .Must(IsValidExpirationDate)
            .WithMessage("Expiration date format is invalid.")
            .Must(IsFutureDate)
            .WithMessage("Expiration date must be in the future.");

        RuleFor(x => x.CVV)
            .NotEmpty()
            .WithMessage("CVV is required.")
            .Matches(@"^\d{3,4}$")
            .WithMessage("CVV must be 3 or 4 digits.");
    }

    private bool IsValidCardNumber(string cardNumber)
    {
        return Regex.IsMatch(cardNumber, @"^\d{13,19}$");
    }

    private bool IsValidExpirationDate(string expirationDate)
    {
        return DateTime.TryParseExact(
            expirationDate,
            new[] { "MM/yy", "MM/yyyy" },
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out _);
    }

    private bool IsFutureDate(string expirationDate)
    {
        if (DateTime.TryParseExact(expirationDate, new[] { "MM/yy", "MM/yyyy" }, 
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var expDate))
        {
            expDate = expDate.AddMonths(1).AddDays(-1);
            return expDate.Date >= DateTime.UtcNow.Date;
        }
        return false;
    }
}
