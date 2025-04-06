using ECommerce.Application.DTO.Request.Account;
using FluentValidation;

namespace ECommerce.Application.Validations.Account;

public class AccountLoginValidation : AbstractValidator<AccountLoginRequestDto>
{
    public AccountLoginValidation()
    {
        RuleFor(x => x.Email)
            .EmailAddress()
            .NotEmpty()
            .WithMessage("Email is required");
        
        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required");
    }
} 