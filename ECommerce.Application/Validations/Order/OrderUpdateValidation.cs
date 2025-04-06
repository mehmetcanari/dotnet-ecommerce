using ECommerce.Application.DTO.Request.Order;
using FluentValidation;

namespace ECommerce.Application.Validations.Order;

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