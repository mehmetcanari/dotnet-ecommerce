using ECommerce.Application.Utility;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Application.Validations.BaseValidator;

public abstract class BaseValidator
{
    public readonly IServiceProvider ServiceProvider;

    protected BaseValidator(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }
    
    protected async Task<Result> ValidateAsync<T>(T dto)
    {
        var validator = GetValidator<T>(); 
        if (validator is null)
        {
            return Result.Success(); // If specific validation is not found, consider it valid
        }

        var validationResult = await RunValidation(validator, dto);
        if (validationResult.IsValid)
        {
            return Result.Success();
        }

        return CreateFailureResult(validationResult.Errors);
    }
    
    protected async Task<Result<T>> ValidateAndReturnAsync<T>(T dto)
    {
        var validator = GetValidator<T>();
        if (validator is null)
        {
            return Result<T>.Success(dto); // If specific validation is not found, consider it valid
        }

        var validationResult = await RunValidation(validator, dto);
        if (validationResult.IsValid)
        {
            return Result<T>.Success(dto);
        }

        return CreateFailureResult<T>(validationResult.Errors);
    }

    private IValidator<T>? GetValidator<T>()
    {
        return ServiceProvider.GetService<IValidator<T>>();
    }

    private static async Task<FluentValidation.Results.ValidationResult> RunValidation<T>(IValidator<T> validator, T dto)
    {
        return await validator.ValidateAsync(dto);
    }

    private static Result CreateFailureResult(IEnumerable<FluentValidation.Results.ValidationFailure> errors)
    {
        var errorMessages = errors
            .Select(e => e.ErrorMessage)
            .ToList();

        var errorMessage = string.Join("; ", errorMessages);
        return Result.Failure(errorMessage);
    }

    private static Result<T> CreateFailureResult<T>(IEnumerable<FluentValidation.Results.ValidationFailure> errors)
    {
        var errorMessages = errors
            .Select(e => e.ErrorMessage)
            .ToList();

        var errorMessage = string.Join("; ", errorMessages);
        return Result<T>.Failure(errorMessage);
    }
}