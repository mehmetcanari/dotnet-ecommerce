using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;

namespace ECommerce.Application.Commands.Category;

public class DeleteCategoryCommand : IRequest<Result>
{
    public required int CategoryId { get; set; }
}

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Result>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILoggingService _logger;

    public DeleteCategoryCommandHandler(ICategoryRepository categoryRepository, ILoggingService logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var categoryResult = await ValidateAndGetCategory(request);
            if (categoryResult.IsFailure)
                return Result.Failure(categoryResult.Error);

            DeleteCategory(categoryResult.Data);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category with ID: {CategoryId}", request.CategoryId);
            return Result.Failure("An unexpected error occurred while deleting the category");
        }
    }

    private async Task<Result<Domain.Model.Category>> ValidateAndGetCategory(DeleteCategoryCommand request)
    {
        var category = await _categoryRepository.GetCategoryById(request.CategoryId);
        if (category == null)
        {
            _logger.LogWarning("Category not found with ID: {CategoryId}", request.CategoryId);
            return Result<Domain.Model.Category>.Failure("Category not found");
        }

        return Result<Domain.Model.Category>.Success(category);
    }

    private void DeleteCategory(Domain.Model.Category category)
    {
        _categoryRepository.Delete(category);
        _logger.LogInformation("Category deleted successfully: {CategoryId}, {CategoryName}", 
            category.CategoryId, category.Name);
    }
}