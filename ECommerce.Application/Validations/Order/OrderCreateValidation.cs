using ECommerce.Application.DTO.Request.Order;
using FluentValidation;

namespace ECommerce.Application.Validations.Order;

public class OrderCreateValidation : AbstractValidator<OrderCreateRequestDto>
{
    public OrderCreateValidation()
    {
        RuleFor(o => o.PaymentCard).NotEmpty().WithMessage("Payment card is required");
    }
}
