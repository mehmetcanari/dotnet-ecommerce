using ECommerce.Application.Commands.Auth;
using FluentValidation;

namespace ECommerce.Application.Validations.Account;

public class AccountLoginValidation : AbstractValidator<LoginCommand>
{
    public AccountLoginValidation()
    {
        RuleFor(x => x.Model.Email).EmailAddress().NotEmpty();

        RuleFor(x => x.Model.Password).NotEmpty();
    }
} 