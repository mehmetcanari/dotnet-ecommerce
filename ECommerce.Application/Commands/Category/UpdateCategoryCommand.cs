using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Category;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Commands.Category;

public class UpdateCategoryCommand(UpdateCategoryRequestDto request) : IRequest<Result>
{
    public readonly UpdateCategoryRequestDto Model = request;
}

public class UpdateCategoryCommandHandler(ICategoryRepository categoryRepository, ILoggingService logger) : IRequestHandler<UpdateCategoryCommand, Result>
{
    public async Task<Result> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var categoryResult = await ValidateAndGetCategory(request);
            if (categoryResult is { IsFailure: true, Message: not null })
                return Result.Failure(categoryResult.Message);

            var nameValidationResult = await ValidateCategoryName(request);
            if (nameValidationResult.IsFailure)
                return nameValidationResult;

            if (categoryResult.Data is null)
                return Result.Failure(ErrorMessages.CategoryNotFound);

            UpdateCategory(categoryResult.Data, request);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.ErrorUpdatingCategory, request.Model.Id);
            return Result.Failure(ErrorMessages.ErrorUpdatingCategory);
        }
    }

    private async Task<Result<Domain.Model.Category>> ValidateAndGetCategory(UpdateCategoryCommand request)
    {
        var category = await categoryRepository.GetById(request.Model.Id);
        if (category == null)
            return Result<Domain.Model.Category>.Failure(ErrorMessages.CategoryNotFound);

        return Result<Domain.Model.Category>.Success(category);
    }

    private async Task<Result> ValidateCategoryName(UpdateCategoryCommand request)
    {
        var categoryExists = await categoryRepository.CheckNameExists(request.Model.Name);
        if (categoryExists)
            return Result.Failure(ErrorMessages.CategoryExists);

        return Result.Success();
    }

    private void UpdateCategory(Domain.Model.Category category, UpdateCategoryCommand request)
    {
        category.Name = request.Model.Name;
        category.Description = request.Model.Description;

        categoryRepository.Update(category);
    }
}