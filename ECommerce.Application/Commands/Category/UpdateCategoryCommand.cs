using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Category;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
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
            if (categoryResult.IsFailure)
                return Result.Failure(categoryResult.Error);

            var nameValidationResult = await ValidateCategoryName(request);
            if (nameValidationResult.IsFailure)
                return nameValidationResult;

            UpdateCategory(categoryResult.Data, request);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category with ID: {CategoryId}", request.CategoryId);
            return Result.Failure("An unexpected error occurred while updating the category");
        }
    }

    private async Task<Result<Domain.Model.Category>> ValidateAndGetCategory(UpdateCategoryCommand request)
    {
        var category = await _categoryRepository.GetCategoryById(request.CategoryId);
        if (category == null)
        {
            _logger.LogWarning("Category not found with ID: {CategoryId}", request.CategoryId);
            return Result<Domain.Model.Category>.Failure("Category not found");
        }

        return Result<Domain.Model.Category>.Success(category);
    }

    private async Task<Result> ValidateCategoryName(UpdateCategoryCommand request)
    {
        var categoryExists = await _categoryRepository.CheckCategoryExistsWithName(request.UpdateCategoryRequestDto.Name);
        if (categoryExists)
        {
            _logger.LogWarning("Category already exists with name: {CategoryName}", request.UpdateCategoryRequestDto.Name);
            return Result.Failure("Category already exists");
        }

        return Result.Success();
    }

    private void UpdateCategory(Domain.Model.Category category, UpdateCategoryCommand request)
    {
        category.Name = request.UpdateCategoryRequestDto.Name;
        category.Description = request.UpdateCategoryRequestDto.Description;

        _categoryRepository.Update(category);
        _logger.LogInformation("Category updated successfully: {CategoryId}, {CategoryName}", 
            category.CategoryId, category.Name);
    }
}