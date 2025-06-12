using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Category;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;

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
            var category = await _categoryRepository.GetCategoryById(request.CategoryId);
            if (category == null)
            {
                return Result.Failure("Category not found");
            }

            category.Name = request.UpdateCategoryRequestDto.Name;
            category.Description = request.UpdateCategoryRequestDto.Description;

            _categoryRepository.Update(category);

            _logger.LogInformation("Category updated successfully");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category");
            return Result.Failure("An unexpected error occurred while updating the category");
        }
    }
}