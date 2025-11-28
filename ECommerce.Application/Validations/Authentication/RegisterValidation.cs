using FluentValidation;
using ECommerce.Application.Commands.Auth;

namespace ECommerce.Application.Validations.Authentication;

public class RegisterValidation : AbstractValidator<RegisterCommand>
{
    public RegisterValidation()
    {

        RuleFor(x => x.Model.Name).NotEmpty().Length(2, 50).Matches(@"^[\p{L}\s\-'\.]+$");

        RuleFor(x => x.Model.Surname).NotEmpty().Length(2, 50).Matches(@"^[\p{L}\s\-'\.]+$");

        RuleFor(x => x.Model.Email).NotEmpty().EmailAddress().MaximumLength(100);

        RuleFor(x => x.Model.IdentityNumber).NotEmpty().Length(11).Matches("^[0-9]+$").Must(IsValidTurkishIdentity);

        RuleFor(x => x.Model.Phone).NotEmpty().Matches(@"^\d{10}$");

        RuleFor(x => x.Model.PhoneCode).NotEmpty().Matches(@"^\d{1,3}$");

        RuleFor(x => x.Model.Password).NotEmpty().MinimumLength(8).MaximumLength(128)
            .Matches("[A-Z]")
            .Matches("[a-z]")
            .Matches("[0-9]")
            .Matches("[^a-zA-Z0-9]");

        RuleFor(x => x.Model.DateOfBirth).NotEmpty().LessThan(DateTime.UtcNow).GreaterThan(DateTime.UtcNow.AddYears(-120)).Must(IsAgeValid);
    }

    private bool IsAgeValid(DateTime dateOfBirth)
    {
        var today = DateTime.UtcNow;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age)) age--;
        return age >= 18;
    }

    private bool IsValidTurkishIdentity(string identityNumber)
    {
        if (identityNumber.Length != 11) return false;
        if (!identityNumber.All(char.IsDigit)) return false;
        if (identityNumber[0] == '0') return false;

        int[] digits = identityNumber.Select(c => int.Parse(c.ToString())).ToArray();

        int oddSum = digits.Take(9).Where((_, i) => i % 2 == 0).Sum();
        int evenSum = digits.Take(9).Where((_, i) => i % 2 == 1).Sum();
        int digit10 = (oddSum * 7 - evenSum) % 10;

        int digit11 = digits.Take(10).Sum() % 10;

        return digits[9] == digit10 && digits[10] == digit11;
    }
}
