using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ECommerce.Application.Validations.Attribute;

public class ValidateIdAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ActionArguments.TryGetValue("id", out var value))
        {
            if (value is <= 0)
            {
                context.Result = new BadRequestObjectResult(new ValidationException("Invalid ID value").Message);
                return;
            }
        }
        base.OnActionExecuting(context);
    }
}