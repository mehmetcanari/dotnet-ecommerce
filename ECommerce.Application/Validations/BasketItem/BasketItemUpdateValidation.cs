using ECommerce.Application.DTO.Request.BasketItem;
using FluentValidation;

namespace ECommerce.Application.Validations.BasketItem;

public class BasketItemUpdateValidation : AbstractValidator<UpdateBasketItemRequestDto>
{
    public BasketItemUpdateValidation()
    {
        RuleFor(x => x.BasketItemId)
            .NotEmpty()
            .WithMessage("Basket Item Id is required.")
            .GreaterThan(0)
            .WithMessage("Basket Item Id must be greater than 0.");
        
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product Id is required.")
            .GreaterThan(0)
            .WithMessage("Product Id must be greater than 0.");
        
        RuleFor(x => x.Quantity)
            .NotEmpty()
            .WithMessage("Quantity is required.")
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0.");
    }
}