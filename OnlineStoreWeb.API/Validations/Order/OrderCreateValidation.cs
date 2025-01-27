using FluentValidation;
using OnlineStoreWeb.API.DTO.Order;

namespace OnlineStoreWeb.API.Validations.Order;

public class OrderCreateValidation : AbstractValidator<OrderCreateDto>
{
    public OrderCreateValidation()
    {
        RuleFor(o => o.UserId)
            .NotEmpty()
            .WithMessage("User ID is required")
            .GreaterThan(0)
            .WithMessage("User ID must be greater than 0");
        
        RuleFor(o => o.ProductId)
            .NotEmpty()
            .WithMessage("Product ID is required")
            .GreaterThan(0)
            .WithMessage("Product ID must be greater than 0");
        
        RuleFor(o => o.Quantity)
            .NotEmpty()
            .WithMessage("Quantity is required")
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0");
        
        RuleFor(o => o.ShippingAddress)
            .NotEmpty()
            .WithMessage("Shipping address is required")
            .Length(10, 100)
            .WithMessage("Shipping address must not exceed 100 characters");
        
        RuleFor(o => o.BillingAddress)
            .NotEmpty()
            .WithMessage("Billing address is required")
            .Length(10, 100)
            .WithMessage("Billing address must not exceed 100 characters");

        RuleFor(o => o.PaymentMethod)
            .NotEmpty()
            .WithMessage("Payment method is required")
            .IsInEnum();
    }
}