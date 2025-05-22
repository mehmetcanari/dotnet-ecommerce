using ECommerce.Application.DTO.Request.Token;
using FluentValidation;

namespace ECommerce.Application.Validations.Token;

public class TokenRevokeValidation : AbstractValidator<TokenRevokeRequestDto>
{
    public TokenRevokeValidation()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email address");
    }
}
