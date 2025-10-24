using ECommerce.Application.DTO.Request.BasketItem;
using FluentValidation;

namespace ECommerce.Application.Validations.BasketItem;

public class BasketItemUpdateValidation : AbstractValidator<UpdateBasketItemRequestDto>
{
    public BasketItemUpdateValidation()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Basket Item Id is required.");

        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product Id is required.");
        
        RuleFor(x => x.Quantity)
            .NotEmpty()
            .WithMessage("Quantity is required.")
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0.");
    }
}