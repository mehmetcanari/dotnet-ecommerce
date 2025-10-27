using ECommerce.Application.Commands.Order;
using FluentValidation;

namespace ECommerce.Application.Validations.Order;

public class OrderStatusUpdateValidation : AbstractValidator<UpdateOrderStatusCommand>
{
    public OrderStatusUpdateValidation()
    {
        RuleFor(o => o.Model.Status).NotEmpty().IsInEnum();
    }
}