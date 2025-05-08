using ECommerce.Application.DTO.Request.Category;
using FluentValidation;

namespace ECommerce.Application.Validations.Category;

public class CategoryDeleteValidation : AbstractValidator<DeleteCategoryRequestDto>
{
    public CategoryDeleteValidation()
    {
        RuleFor(c => c.CategoryId)
            .NotEmpty()
            .WithMessage("CategoryId is required")
            .GreaterThan(0)
            .WithMessage("CategoryId must be greater than 0");
    }
}