using ECommerce.Application.Commands.Product;
using FluentValidation;

namespace ECommerce.Application.Validations.Product;

public class ProductUpdateValidation : AbstractValidator<UpdateProductCommand>
{
    public ProductUpdateValidation()
    {
        RuleFor(p => p.Model.Name).NotEmpty().MaximumLength(50);

        RuleFor(p => p.Model.ImageUrl).MaximumLength(200);

        RuleFor(p => p.Model.Description).NotEmpty().MaximumLength(200);

        RuleFor(p => p.Model.Price).NotEmpty().GreaterThan(0);

        RuleFor(p => p.Model.StockQuantity).NotEmpty().GreaterThan(0);
    }
}