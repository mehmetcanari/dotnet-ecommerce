using ECommerce.Application.Commands.Account;
using FluentValidation;

namespace ECommerce.Application.Validations.Account;

public class AccountBanValidation : AbstractValidator<BanAccountCommand>
{
    public AccountBanValidation()
    {
        RuleFor(x => x.Model.Email).NotEmpty().EmailAddress();

        RuleFor(x => x.Model.Until).NotEmpty().LessThan(DateTime.UtcNow);

        RuleFor(x => x.Model.Reason).NotEmpty().MaximumLength(200);
    }
}
