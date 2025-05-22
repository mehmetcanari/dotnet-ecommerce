using ECommerce.Application.DTO.Request.Account;
using FluentValidation;

namespace ECommerce.Application.Validations.Account;

public class AccountUnbanValidation : AbstractValidator<AccountUnbanRequestDto>
{
    public AccountUnbanValidation()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("You must provide an email address for the account to be unbanned")
            .EmailAddress()
            .WithMessage("The email address provided is invalid");
    }
}
