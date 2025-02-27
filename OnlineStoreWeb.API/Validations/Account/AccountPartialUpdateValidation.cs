using FluentValidation;
using OnlineStoreWeb.API.DTO.Request.Account;

namespace OnlineStoreWeb.API.Validations.Account;

public class AccountPartialUpdateValidation : AbstractValidator<AccountPatchDto>
{
    public AccountPartialUpdateValidation()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email is not valid");

        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required");
    }
}