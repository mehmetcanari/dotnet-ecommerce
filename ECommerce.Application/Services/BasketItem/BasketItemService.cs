using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.BasketItem;
using ECommerce.Application.Validations.BaseValidator;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;
using ECommerce.Application.Commands.Basket;
using ECommerce.Shared.Constants;

namespace ECommerce.Application.Services.BasketItem;

public class BasketItemService : BaseValidator, IBasketItemService
{
    private readonly IBasketItemRepository _basketItemRepository;
    private readonly ICacheService _cacheService;
    private readonly ILoggingService _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private IMediator _mediator;

    public BasketItemService(
        IBasketItemRepository basketItemRepository, 
        ILoggingService logger,
        ICacheService cacheService,
        IUnitOfWork unitOfWork,
        IServiceProvider serviceProvider, 
        ICurrentUserService currentUserService,
        IMediator mediator) : base(serviceProvider)
    {
        _basketItemRepository = basketItemRepository;
        _logger = logger;
        _cacheService = cacheService;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mediator = mediator;
    }

    public async Task<Result> CreateBasketItemAsync(CreateBasketItemRequestDto createBasketItemRequestDto)
    {
        try
        {
            var validationResult = await ValidateAsync(createBasketItemRequestDto);
            if (validationResult is { IsSuccess: false, Error: not null }) 
                return Result.Failure(validationResult.Error);

            await ClearBasketItemsCacheAsync();
            
            var result = await _mediator.Send(new CreateBasketItemCommand { CreateBasketItemRequestDto = createBasketItemRequestDto });
            if (result is { IsSuccess: false, Error: not null })
            {
                _logger.LogWarning(ErrorMessages.ErrorAddingItemToBasket, result.Error);
                return Result.Failure(result.Error);
            }

            await _unitOfWork.Commit();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.UnexpectedError);
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result> UpdateBasketItemAsync(UpdateBasketItemRequestDto updateBasketItemRequestDto)
    {
        try
        {
            var validationResult = await ValidateAsync(updateBasketItemRequestDto);
            if (validationResult is { IsSuccess: false, Error: not null }) 
                return Result.Failure(validationResult.Error);

            var result = await _mediator.Send(new UpdateBasketItemCommand { UpdateBasketItemRequestDto = updateBasketItemRequestDto });
            if (result is { IsSuccess: false, Error: not null })
            {
                _logger.LogWarning(ErrorMessages.ErrorUpdatingBasketItem, result.Error);
                return Result.Failure(result.Error);
            }

            await ClearBasketItemsCacheAsync();
            await _unitOfWork.Commit();

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.UnexpectedError);
            return Result.Failure(ex.Message);
        }
    }

    public async Task<Result> DeleteAllNonOrderedBasketItemsAsync()
    {
        try
        {
            var result = await _mediator.Send(new DeleteAllNonOrderedBasketItemsCommand());
            if (result is { IsSuccess: false, Error: not null })
                return Result.Failure(result.Error);

            await ClearBasketItemsCacheAsync();
            await _unitOfWork.Commit();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.UnexpectedError);
            return Result.Failure(ex.Message);
        }
    }

    public async Task ClearBasketItemsIncludeOrderedProductAsync(Domain.Model.Product updatedProduct)
    {
        try
        {
            var nonOrderedBasketItems = await _basketItemRepository.GetNonOrderedBasketItemIncludeSpecificProduct(updatedProduct.ProductId);
            if (nonOrderedBasketItems == null || nonOrderedBasketItems.Count == 0)
                return;

            foreach (var basketItem in nonOrderedBasketItems)
            {
                _basketItemRepository.Delete(basketItem);
            }

            await ClearBasketItemsCacheAsync();
            await _unitOfWork.Commit();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.UnexpectedError);
            throw;
        }
    }

    public async Task ClearBasketItemsCacheAsync()
    {
        await _cacheService.RemoveAsync($"{CacheKeys.AllBasketItems}_{_currentUserService.GetUserEmail()}");
    }
}