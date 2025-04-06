using ECommerce.API.DTO.Request.Account;
using FluentValidation;

namespace ECommerce.API.Validations.Account;

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