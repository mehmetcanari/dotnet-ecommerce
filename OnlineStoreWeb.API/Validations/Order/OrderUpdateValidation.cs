using FluentValidation;
using OnlineStoreWeb.API.DTO.Request.Order;

namespace OnlineStoreWeb.API.Validations.Order;

public class OrderUpdateValidation : AbstractValidator<OrderUpdateRequestDto>
{
    public OrderUpdateValidation()
    {
        RuleFor(o => o.Status)
            .NotEmpty()
            .WithMessage("Status is required")
            .IsInEnum();
    }
}