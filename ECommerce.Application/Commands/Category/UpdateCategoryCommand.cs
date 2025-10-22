using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Category;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Commands.Category;

public class UpdateCategoryCommand : IRequest<Result>
{
    public required int CategoryId { get; set; }
    public required UpdateCategoryRequestDto UpdateCategoryRequestDto { get; set; }
}

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Result>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILoggingService _logger;

    public UpdateCategoryCommandHandler(ICategoryRepository categoryRepository, ILoggingService logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var categoryResult = await ValidateAndGetCategory(request);
            if (categoryResult.IsFailure && categoryResult.Error is not null)
                return Result.Failure(categoryResult.Error);

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
            _logger.LogError(ex, ErrorMessages.ErrorUpdatingCategory, request.CategoryId);
            return Result.Failure(ErrorMessages.ErrorUpdatingCategory);
        }
    }

    private async Task<Result<Domain.Model.Category>> ValidateAndGetCategory(UpdateCategoryCommand request)
    {
        var category = await _categoryRepository.GetCategoryById(request.CategoryId);
        if (category == null)
        {
            return Result<Domain.Model.Category>.Failure(ErrorMessages.CategoryNotFound);
        }

        return Result<Domain.Model.Category>.Success(category);
    }

    private async Task<Result> ValidateCategoryName(UpdateCategoryCommand request)
    {
        var categoryExists = await _categoryRepository.CheckCategoryExistsWithName(request.UpdateCategoryRequestDto.Name);
        if (categoryExists)
        {
            return Result.Failure(ErrorMessages.CategoryExists);
        }

        return Result.Success();
    }

    private void UpdateCategory(Domain.Model.Category category, UpdateCategoryCommand request)
    {
        category.Name = request.UpdateCategoryRequestDto.Name;
        category.Description = request.UpdateCategoryRequestDto.Description;

        _categoryRepository.Update(category);
    }
}