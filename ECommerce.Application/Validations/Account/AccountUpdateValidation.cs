using ECommerce.Application.Commands.Account;
using FluentValidation;

namespace ECommerce.Application.Validations.Account
{
    public class AccountUpdateValidation : AbstractValidator<UpdateProfileCommand>
    {
        public AccountUpdateValidation()
        {
            When(x => !string.IsNullOrWhiteSpace(x.Model.Name), () =>
            {
                RuleFor(x => x.Model.Name)
                    .Length(2, 50)
                    .Matches(@"^[\p{L}\s\-'\.]+$");
            });

            When(x => !string.IsNullOrWhiteSpace(x.Model.Surname), () =>
            {
                RuleFor(x => x.Model.Surname)
                    .Length(2, 50)
                    .Matches(@"^[\p{L}\s\-'\.]+$");
            });

            When(x => !string.IsNullOrWhiteSpace(x.Model.Email), () =>
            {
                RuleFor(x => x.Model.Email)
                    .EmailAddress()
                    .MaximumLength(100);
            });

            When(x => !string.IsNullOrWhiteSpace(x.Model.Country), () =>
            {
                RuleFor(x => x.Model.Country)
                    .Length(2, 50);
            });

            When(x => !string.IsNullOrWhiteSpace(x.Model.City), () =>
            {
                RuleFor(x => x.Model.City)
                    .Length(2, 50);
            });

            When(x => !string.IsNullOrWhiteSpace(x.Model.ZipCode), () =>
            {
                RuleFor(x => x.Model.ZipCode)
                    .Length(5, 10)
                    .Matches(@"^[0-9a-zA-Z\-\s]+$");
            });

            When(x => !string.IsNullOrWhiteSpace(x.Model.Address), () =>
            {
                RuleFor(x => x.Model.Address)
                    .Length(5, 200);
            });

            When(x => !string.IsNullOrWhiteSpace(x.Model.PhoneCode), () =>
            {
                RuleFor(x => x.Model.PhoneCode)
                    .Matches(@"^\d{1,3}$");
            });

            When(x => !string.IsNullOrWhiteSpace(x.Model.PhoneNumber), () =>
            {
                RuleFor(x => x.Model.PhoneNumber)
                    .Matches(@"^\d{10}$");
            });

            When(x => !string.IsNullOrWhiteSpace(x.Model.Password), () =>
            {
                RuleFor(x => x.Model.Password)
                    .MinimumLength(8).MaximumLength(128)
                    .Matches("[A-Z]")
                    .Matches("[a-z]")
                    .Matches("[0-9]")
                    .Matches("[^a-zA-Z0-9]");

                RuleFor(x => x.Model.OldPassword)
                    .NotEmpty()
                    .WithMessage("Old password is required when setting a new password.");
            });

            When(x => !string.IsNullOrWhiteSpace(x.Model.OldPassword), () =>
            {
                RuleFor(x => x.Model.Password)
                    .NotEmpty()
                    .WithMessage("New password is required when old password is provided.");
            });
        }
    }
}
