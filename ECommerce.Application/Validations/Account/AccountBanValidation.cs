using ECommerce.Application.DTO.Request.Account;
using FluentValidation;

namespace ECommerce.Application.Validations.Account;

public class AccountBanValidation : AbstractValidator<AccountBanRequestDto>
{
    public AccountBanValidation()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("You must provide an email address for the account to be banned")
            .EmailAddress()
            .WithMessage("The email address provided is invalid");

        RuleFor(x => x.Until)
            .NotEmpty()
            .WithMessage("You must specify a date and time for the ban to take effect")
            .LessThan(DateTime.UtcNow)
            .WithMessage("The date and time for the ban to take effect must be in the future");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("You must provide a reason for the ban")
            .MaximumLength(200)
            .WithMessage("The reason for the ban must be less than 200 characters");
    }
}
