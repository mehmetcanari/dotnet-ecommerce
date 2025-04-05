using ECommerce.API.DTO.Request.OrderItem;
using FluentValidation;

namespace ECommerce.API.Validations.OrderItem;

public class OrderItemCreateValidation : AbstractValidator<CreateOrderItemRequestDto>
{
    public OrderItemCreateValidation()
    {        
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