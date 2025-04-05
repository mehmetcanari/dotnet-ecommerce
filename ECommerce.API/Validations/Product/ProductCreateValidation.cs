using ECommerce.API.DTO.Request.Product;
using FluentValidation;

namespace ECommerce.API.Validations.Product;

public class ProductCreateValidation : AbstractValidator<ProductCreateRequestDto>
{
    public ProductCreateValidation()
    {
        RuleFor(p => p.Name)
            .NotEmpty()
            .WithMessage("Product name is required")
            .MaximumLength(50)
            .WithMessage("Product name cannot be more than 50 characters");
        
        RuleFor(p => p.ImageUrl)
            .MaximumLength(200)
            .WithMessage("Product image URL cannot be more than 200 characters");

        RuleFor(p => p.Description)
            .NotEmpty()
            .WithMessage("Product description is required")
            .MaximumLength(200)
            .WithMessage("Product description cannot be more than 200 characters");

        RuleFor(p => p.Price)
            .NotEmpty()
            .WithMessage("Product price is required")
            .GreaterThan(0)
            .WithMessage("Product price must be greater than 0");

        RuleFor(p => p.StockQuantity)
            .NotEmpty()
            .WithMessage("Product quantity is required")
            .GreaterThan(0)
            .WithMessage("Product quantity must be greater than 0");
    }
}