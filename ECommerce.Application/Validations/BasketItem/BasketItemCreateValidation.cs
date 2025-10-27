using ECommerce.Application.Commands.Basket;
using FluentValidation;

namespace ECommerce.Application.Validations.BasketItem;

public class BasketItemCreateValidation : AbstractValidator<CreateBasketItemCommand>
{
    public BasketItemCreateValidation()
    {
        RuleFor(x => x.Model.ProductId).NotEmpty();

        RuleFor(x => x.Model.Quantity).NotEmpty().GreaterThan(0);
    }
}