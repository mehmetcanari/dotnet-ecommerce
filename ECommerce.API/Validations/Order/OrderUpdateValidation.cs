using ECommerce.API.DTO.Request.Order;
using FluentValidation;

namespace ECommerce.API.Validations.Order;

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