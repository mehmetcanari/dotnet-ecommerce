using ECommerce.Application.Commands.Basket;
using FluentValidation;

namespace ECommerce.Application.Validations.BasketItem;

public class BasketItemUpdateValidation : AbstractValidator<UpdateBasketItemCommand>
{
    public BasketItemUpdateValidation()
    {
        RuleFor(x => x.Model.Id).NotEmpty();

        RuleFor(x => x.Model.ProductId).NotEmpty();

        RuleFor(x => x.Model.Quantity).NotEmpty().GreaterThan(0);
    }
}