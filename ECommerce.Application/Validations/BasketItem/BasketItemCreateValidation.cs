using ECommerce.Application.DTO.Request.BasketItem;
using FluentValidation;

namespace ECommerce.Application.Validations.BasketItem;

public class BasketItemCreateValidation : AbstractValidator<CreateBasketItemRequestDto>
{
    public BasketItemCreateValidation()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Product id is required.");
        
        RuleFor(x => x.Quantity)
            .NotEmpty()
            .WithMessage("Quantity is required.")
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0.");
    }
}