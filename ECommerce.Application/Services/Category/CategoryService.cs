using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Category;
using ECommerce.Application.Validations.BaseValidator;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;
using ECommerce.Application.Commands.Category;

namespace ECommerce.Application.Services.Category;

public class CategoryService : BaseValidator, ICategoryService
{
    private const string CategoryCacheKey = "category:{0}";
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILoggingService _logger;
    private readonly ICacheService _cacheService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;


    public CategoryService(
        IServiceProvider serviceProvider, 
        ICategoryRepository categoryRepository, 
        ILoggingService logger, 
        ICacheService cacheService, 
        IUnitOfWork unitOfWork, 
        IMediator mediator) : base(serviceProvider)
    {
        _unitOfWork = unitOfWork;
        _categoryRepository = categoryRepository;
        _logger = logger;
        _cacheService = cacheService;
        _mediator = mediator;
    }

    public async Task<Result> CreateCategoryAsync(CreateCategoryRequestDto request)
    {
        try
        {
            var validationResult = await ValidateAsync(request);
            if (!validationResult.IsSuccess)
            {
                return validationResult;
            }
            
            var commandResult = await _mediator.Send(new CreateCategoryCommand
            {
                CreateCategoryRequestDto = request
            });

            if (commandResult is { IsSuccess: false, Error: not null })
            {
                _logger.LogWarning("Failed to create category: {Error}", commandResult.Error);
                return Result.Failure(commandResult.Error);
            }

            await _unitOfWork.Commit();
            await CategoryCacheInvalidateAsync();
            _logger.LogInformation("Category created successfully");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result> DeleteCategoryAsync(int categoryId)
    {
        try
        {
            var result = await _mediator.Send(new DeleteCategoryCommand
            {
                CategoryId = categoryId
            });
            if (result is { IsSuccess: false, Error: not null })
            {
                _logger.LogWarning("Failed to delete category: {Error}", result.Error);
                return Result.Failure(result.Error);
            }

            await CategoryCacheInvalidateAsync();
            await _unitOfWork.Commit();
            _logger.LogInformation("Category deleted successfully");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category");
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result> UpdateCategoryAsync(int categoryId, UpdateCategoryRequestDto request)
    {
        try
        {
            var result = await ValidateAsync(request);
            if (!result.IsSuccess)
            {
                return result;
            }
            
            var commandResult = await _mediator.Send(new UpdateCategoryCommand
            {
                CategoryId = categoryId,
                UpdateCategoryRequestDto = request
            });
            
            if (commandResult is { IsSuccess: false, Error: not null })
            {
                _logger.LogWarning("Failed to update category: {Error}", commandResult.Error);
                return Result.Failure(commandResult.Error);
            }

            await CategoryCacheInvalidateAsync();
            await _unitOfWork.Commit();
            _logger.LogInformation("Category updated successfully");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category");
            return Result.Failure(ex.Message);
        }
    }

    public async Task CategoryCacheInvalidateAsync()
    {   
        try
        {
            var categories = await _categoryRepository.Read();
            foreach (var category in categories)
            {
                await _cacheService.RemoveAsync(CategoryCacheKey);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating category cache");
            throw;
        }
    }
}