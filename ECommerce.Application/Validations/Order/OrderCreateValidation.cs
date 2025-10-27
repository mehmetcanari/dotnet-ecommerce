using ECommerce.Application.Commands.Order;
using FluentValidation;

namespace ECommerce.Application.Validations.Order;

public class OrderCreateValidation : AbstractValidator<CreateOrderCommand>
{
    public OrderCreateValidation()
    {
        RuleFor(o => o.Model.PaymentCard).NotEmpty();
    }
}
