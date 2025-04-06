using ECommerce.Application.DTO.Request.Account;
using FluentValidation;

namespace ECommerce.Application.Validations.Account;

public class AccountUpdateValidation : AbstractValidator<AccountUpdateRequestDto>
{
    public AccountUpdateValidation()
    {
        RuleFor(x => x.FullName)
            .Length(2, 50)
            .NotEmpty()
            .WithMessage("Full name is required");
        
        RuleFor(x => x.Email)
            .EmailAddress()
            .NotEmpty()
            .WithMessage("Email is required");
        
        RuleFor(x => x.Password)
            .Length(6, 50)
            .NotEmpty()
            .WithMessage("Password is required");
        
        RuleFor(x => x.Address)
            .Length(2, 100)
            .NotEmpty()
            .WithMessage("Address is required");
        
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number is required")
            .Length(10,10)
            .WithMessage("Phone number must be 10 digits");
    }
}