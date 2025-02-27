using FluentValidation;
using OnlineStoreWeb.API.DTO.Request.Order;

namespace OnlineStoreWeb.API.Validations.Order;

public class OrderCreateValidation : AbstractValidator<OrderCreateDto>
{
    public OrderCreateValidation()
    {
        RuleFor(o => o.AccountId)
            .NotEmpty()
            .WithMessage("User ID is required")
            .GreaterThan(0)
            .WithMessage("User ID must be greater than 0");
        
        RuleFor(o => o.ShippingAddress)
            .NotEmpty()
            .WithMessage("Shipping address is required")
            .Length(10, 100)
            .WithMessage("Shipping address must between 10 and 100 characters");
        
        RuleFor(o => o.BillingAddress)
            .NotEmpty()
            .WithMessage("Billing address is required")
            .Length(10, 100)
            .WithMessage("Billing address must between 10 and 100 characters");

        RuleFor(o => o.PaymentMethod)
            .NotEmpty()
            .WithMessage("Payment method is required")
            .IsInEnum();
    }
}