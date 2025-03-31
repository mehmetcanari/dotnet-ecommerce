using FluentValidation;
using OnlineStoreWeb.API.DTO.Request.Account;

namespace OnlineStoreWeb.API.Validations.Account;

public class AccountLoginValidation : AbstractValidator<AccountLoginDto>
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