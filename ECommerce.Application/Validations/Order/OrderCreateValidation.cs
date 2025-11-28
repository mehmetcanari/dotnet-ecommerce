using ECommerce.Application.Commands.Order;
using FluentValidation;

namespace ECommerce.Application.Validations.Order;

public class OrderCreateValidation : AbstractValidator<CreateOrderCommand>
{
    public OrderCreateValidation()
    {
        RuleFor(x => x.Model.CardHolderName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Model.CardNumber).NotEmpty().CreditCard();
        RuleFor(x => x.Model.ExpirationMonth).InclusiveBetween(1, 12);
        RuleFor(x => x.Model.ExpirationYear).GreaterThanOrEqualTo(DateTime.Now.Year);
        RuleFor(x => x.Model.Cvc).NotEmpty().Length(3, 4);
    }
}
