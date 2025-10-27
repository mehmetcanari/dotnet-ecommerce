using ECommerce.Application.Commands.Auth;
using FluentValidation;

namespace ECommerce.Application.Validations.Authentication;

public class LoginValidation : AbstractValidator<LoginCommand>
{
    public LoginValidation()
    {
        RuleFor(x => x.Model.Email).EmailAddress().NotEmpty();

        RuleFor(x => x.Model.Password).NotEmpty();
    }
} 