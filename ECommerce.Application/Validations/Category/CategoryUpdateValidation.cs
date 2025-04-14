using ECommerce.Application.DTO.Request.Category;
using FluentValidation;

public class CategoryUpdateValidation : AbstractValidator<UpdateCategoryRequestDto>
{
    public CategoryUpdateValidation()
    {
        RuleFor(c => c.Name)
        .NotEmpty()
        .WithMessage("Name is required")
        .MinimumLength(3)
        .WithMessage("Name must be at least 3 characters long")
        .MaximumLength(100)
        .WithMessage("Name must be less than 100 characters long");

        RuleFor(c => c.Description)
        .NotEmpty()
        .WithMessage("Description is required")
        .MinimumLength(3)
        .WithMessage("Description must be at least 3 characters long")
        .MaximumLength(200)
        .WithMessage("Description must be less than 200 characters long");
    }
}