using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Application.Validations;

public static class ValidationDependency
{
    public static void AddValidationDependencies(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(ValidationDependency).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    }
}