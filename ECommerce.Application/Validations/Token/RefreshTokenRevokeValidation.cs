using ECommerce.Application.Commands.Token;
using FluentValidation;

namespace ECommerce.Application.Validations.Token;

public class RefreshTokenRevokeValidation : AbstractValidator<RevokeRefreshTokenCommand>
{
    public RefreshTokenRevokeValidation()
    {
        RuleFor(x => x.Model.Email).NotEmpty().EmailAddress();
    }
}
