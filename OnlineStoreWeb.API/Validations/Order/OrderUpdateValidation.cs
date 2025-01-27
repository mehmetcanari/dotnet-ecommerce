using FluentValidation;
using OnlineStoreWeb.API.DTO.Order;

namespace OnlineStoreWeb.API.Validations.Order;

public class OrderUpdateValidation : AbstractValidator<OrderUpdateDto>
{
    public OrderUpdateValidation()
    {
        RuleFor(o => o.Status)
            .NotEmpty()
            .WithMessage("Status is required")
            .IsInEnum();
    }
}