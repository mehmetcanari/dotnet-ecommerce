using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;

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
            var category = await _categoryRepository.GetCategoryById(request.CategoryId);
            if (category == null)
            {
                return Result.Failure("Category not found");
            }

            _categoryRepository.Delete(category);
            _logger.LogInformation("Category deleted successfully");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category");
            return Result.Failure("An unexpected error occurred while deleting the category");
        }
    }
}