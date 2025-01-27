using FluentValidation;
using OnlineStoreWeb.API.DTO.User;

namespace OnlineStoreWeb.API.Validations.Account;

public class AccountRegisterValidation : AbstractValidator<AccountRegisterDto>
{
    public AccountRegisterValidation ()
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
            .GreaterThan(0)
            .NotEmpty()
            .WithMessage("Phone number is required")
            .Must(IsPhoneNumberValid)
            .WithMessage("Phone number must be 10 digits");

        RuleFor(x => x.DateOfBirth)
            .InclusiveBetween(new DateTime(1900, 1, 1), DateTime.Now)
            .NotEmpty()
            .WithMessage("Date of birth is required")
            .Must(IsAgeValid)
            .WithMessage("You must be 18 years or older to register");
        
        
        bool IsAgeValid(DateTime dateOfBirth)
        {
            return dateOfBirth.AddYears(18) <= DateTime.Now;
        }
        
        bool IsPhoneNumberValid(int phoneNumber)
        {
            return phoneNumber.ToString().Length == 10;
        }
    }
}