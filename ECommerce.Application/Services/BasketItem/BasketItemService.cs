using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.BasketItem;
using ECommerce.Application.Validations.BaseValidator;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;
using ECommerce.Application.Commands.Basket;

namespace ECommerce.Application.Services.BasketItem;

public class BasketItemService : BaseValidator, IBasketItemService
{
    private readonly IBasketItemRepository _basketItemRepository;
    private readonly ICacheService _cacheService;
    private readonly ILoggingService _logger;
    private readonly IUnitOfWork _unitOfWork;
    private IMediator _mediator;
    private const string GetAllBasketItemsCacheKey = "GetAllBasketItems";

    public BasketItemService(
        IBasketItemRepository basketItemRepository, 
        ILoggingService logger,
        ICacheService cacheService,
        IUnitOfWork unitOfWork,
        IServiceProvider serviceProvider, 
        IMediator mediator) : base(serviceProvider)
    {
        _basketItemRepository = basketItemRepository;
        _logger = logger;
        _cacheService = cacheService;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }

    public async Task<Result> CreateBasketItemAsync(CreateBasketItemRequestDto createBasketItemRequestDto)
    {
        try
        {
            var validationResult = await ValidateAsync(createBasketItemRequestDto);
            if (validationResult is { IsSuccess: false, Error: not null }) 
                return Result.Failure(validationResult.Error);

            var result = await _mediator.Send(new CreateBasketItemCommand { CreateBasketItemRequestDto = createBasketItemRequestDto });
            if (result is { IsSuccess: false, Error: not null })
            {
                _logger.LogWarning("Failed to create basket item: {Error}", result.Error);
                return Result.Failure(result.Error);
            }

            await ClearBasketItemsCacheAsync();
            await _unitOfWork.Commit();
            return Result.Success();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while creating basket item");
            return Result.Failure(exception.Message);
        }
    }

    public async Task<Result> UpdateBasketItemAsync(UpdateBasketItemRequestDto updateBasketItemRequestDto)
    {
        try
        {
            var validationResult = await ValidateAsync(updateBasketItemRequestDto);
            if (validationResult is { IsSuccess: false, Error: not null }) 
                return Result.Failure(validationResult.Error);

            //mediatr process
            var result = await _mediator.Send(new UpdateBasketItemCommand { UpdateBasketItemRequestDto = updateBasketItemRequestDto });
            if (result is { IsSuccess: false, Error: not null })
            {
                _logger.LogWarning("Failed to update basket item: {Error}", result.Error);
                return Result.Failure(result.Error);
            }

            await ClearBasketItemsCacheAsync();
            await _unitOfWork.Commit();

            return Result.Success();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while updating basket item");
            throw;
        }
    }

    public async Task<Result> DeleteAllNonOrderedBasketItemsAsync()
    {
        try
        {
            var result = await _mediator.Send(new DeleteAllNonOrderedBasketItemsCommand());
            if (result is { IsSuccess: false, Error: not null })
            {
                _logger.LogWarning("Failed to delete all non-ordered basket items: {Error}", result.Error);
                return Result.Failure(result.Error);
            }

            await ClearBasketItemsCacheAsync();
            await _unitOfWork.Commit();

            return Result.Success();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while deleting basket items");
            throw;
        }
    }

    public async Task ClearBasketItemsIncludeOrderedProductAsync(Domain.Model.Product updatedProduct)
    {
        try
        {
            var nonOrderedBasketItems = await _basketItemRepository.GetNonOrderedBasketItemIncludeSpecificProduct(updatedProduct.ProductId);
            if (nonOrderedBasketItems == null || nonOrderedBasketItems.Count == 0)
            {
                _logger.LogInformation("No non-ordered basket items found for product: {ProductId}", updatedProduct.ProductId);
                return;
            }

            foreach (var basketItem in nonOrderedBasketItems)
            {
                _basketItemRepository.Delete(basketItem);
            }

            await ClearBasketItemsCacheAsync();
            await _unitOfWork.Commit();
            _logger.LogInformation("Basket items cleared successfully for product: {ProductId}", updatedProduct.ProductId);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while clearing basket items include product");
            throw;
        }
    }

    private async Task ClearBasketItemsCacheAsync()
    {
        await _cacheService.RemoveAsync(GetAllBasketItemsCacheKey);
    }
}