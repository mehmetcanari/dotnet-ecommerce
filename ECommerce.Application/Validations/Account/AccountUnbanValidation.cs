using ECommerce.Application.Commands.Account;
using FluentValidation;

namespace ECommerce.Application.Validations.Account;

public class AccountUnbanValidation : AbstractValidator<UnbanAccountCommand>
{
    public AccountUnbanValidation()
    {
        RuleFor(x => x.Model.Email).NotEmpty().EmailAddress();
    }
}
