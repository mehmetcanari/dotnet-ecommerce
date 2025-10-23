using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Category;
using ECommerce.Application.Validations.BaseValidator;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;
using ECommerce.Application.Commands.Category;
using ECommerce.Shared.Constants;

namespace ECommerce.Application.Services.Category;

public class CategoryService : BaseValidator, ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILoggingService _logger;
    private readonly ICacheService _cacheService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;


    public CategoryService(IServiceProvider serviceProvider, ICategoryRepository categoryRepository, ILoggingService logger, ICacheService cacheService, 
        IUnitOfWork unitOfWork, IMediator mediator) : base(serviceProvider)
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
                return validationResult;
            
            var commandResult = await _mediator.Send(new CreateCategoryCommand
            {
                CreateCategoryRequestDto = request
            });

            if (commandResult is { IsSuccess: false, Message: not null })
                return Result.Failure(commandResult.Message);

            await _unitOfWork.Commit();
            await CategoryCacheInvalidateAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.ErrorCreatingCategory, ex.Message);
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

            if (result is { IsSuccess: false, Message: not null })
                return Result.Failure(result.Message);

            await CategoryCacheInvalidateAsync();
            await _unitOfWork.Commit();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.ErrorDeletingCategory, ex.Message);
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
            
            if (commandResult is { IsSuccess: false, Message: not null })
                return Result.Failure(commandResult.Message);

            await CategoryCacheInvalidateAsync();
            await _unitOfWork.Commit();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.ErrorUpdatingCategory, ex.Message);
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
                await _cacheService.RemoveAsync(string.Format(CacheKeys.CategoryById, category.CategoryId));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.UnexpectedCacheError, ex.Message);
            return;
        }
    }
}