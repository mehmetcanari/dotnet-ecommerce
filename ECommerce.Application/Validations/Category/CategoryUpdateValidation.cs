using ECommerce.Application.Commands.Category;
using FluentValidation;

namespace ECommerce.Application.Validations.Category;

public class CategoryUpdateValidation : AbstractValidator<UpdateCategoryCommand>
{
    public CategoryUpdateValidation()
    {
        RuleFor(c => c.Model.Name).NotEmpty().MinimumLength(3).MaximumLength(100);

        RuleFor(c => c.Model.Description).MinimumLength(3).MaximumLength(200);
    }
}