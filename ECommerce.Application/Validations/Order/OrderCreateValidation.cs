using ECommerce.Application.DTO.Request.Order;
using FluentValidation;

namespace ECommerce.Application.Validations.Order;

public class OrderCreateValidation : AbstractValidator<OrderCreateRequestDto>
{
    public OrderCreateValidation()
    {   
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