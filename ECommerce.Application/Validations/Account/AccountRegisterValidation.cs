using ECommerce.Application.DTO.Request.Account;
using FluentValidation;
using System.Text.RegularExpressions;

namespace ECommerce.Application.Validations.Account;

public class AccountRegisterValidation : AbstractValidator<AccountRegisterRequestDto>
{
    public AccountRegisterValidation()
    {

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .Length(2, 50).WithMessage("Name must be between 2 and 50 characters")
            .Matches(@"^[\p{L}\s\-'\.]+$").WithMessage("Name can only contain letters, spaces, hyphens, apostrophes and dots");

        RuleFor(x => x.Surname)
            .NotEmpty().WithMessage("Surname is required")
            .Length(2, 50).WithMessage("Surname must be between 2 and 50 characters")
            .Matches(@"^[\p{L}\s\-'\.]+$").WithMessage("Surname can only contain letters, spaces, hyphens, apostrophes and dots");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters");

        RuleFor(x => x.IdentityNumber)
            .NotEmpty().WithMessage("Identity number is required")
            .Length(11).WithMessage("Identity number must be 11 digits")
            .Matches(@"^[0-9]+$").WithMessage("Identity number must contain only numbers")
            .Must(IsValidTurkishIdentity).WithMessage("Invalid identity number format");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required")
            .Length(2, 50).WithMessage("Country must be between 2 and 50 characters");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required")
            .Length(2, 50).WithMessage("City must be between 2 and 50 characters");

        RuleFor(x => x.ZipCode)
            .NotEmpty().WithMessage("Zip code is required")
            .Length(5, 10).WithMessage("Zip code must be between 5 and 10 characters")
            .Matches(@"^[0-9a-zA-Z\-\s]+$").WithMessage("Invalid zip code format");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required")
            .Length(5, 200).WithMessage("Address must be between 5 and 200 characters");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Phone number must be in international format (E.164)")
            .Must(IsValidPhoneNumber).WithMessage("Invalid phone number format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .MaximumLength(128).WithMessage("Password must not exceed 128 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one number")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required")
            .LessThan(DateTime.UtcNow).WithMessage("Date of birth cannot be in the future")
            .GreaterThan(DateTime.UtcNow.AddYears(-120)).WithMessage("Age cannot exceed 120 years")
            .Must(IsAgeValid).WithMessage("You must be 18 years or older to register");
    }

    private bool IsAgeValid(DateTime dateOfBirth)
    {
        var today = DateTime.UtcNow;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age)) age--;
        return age >= 18;
    }

    private bool IsValidPhoneNumber(string phoneNumber)
    {
        var digitsOnly = Regex.Replace(phoneNumber, @"[^\d+]", "");
        
        if (digitsOnly.StartsWith("+"))
        {
            digitsOnly = digitsOnly.Substring(1);
        }
        
        return digitsOnly.Length >= 8 && digitsOnly.Length <= 15;
    }

    private bool IsValidTurkishIdentity(string identityNumber)
    {
        if (identityNumber.Length != 11) return false;
        if (!identityNumber.All(char.IsDigit)) return false;
        if (identityNumber[0] == '0') return false;

        int[] digits = identityNumber.Select(c => int.Parse(c.ToString())).ToArray();

        int oddSum = digits.Take(9).Where((x, i) => i % 2 == 0).Sum();
        int evenSum = digits.Take(9).Where((x, i) => i % 2 == 1).Sum();
        int digit10 = (oddSum * 7 - evenSum) % 10;

        int digit11 = digits.Take(10).Sum() % 10;

        return digits[9] == digit10 && digits[10] == digit11;
    }
}