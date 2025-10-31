using ECommerce.Application.Abstract;
using ECommerce.Application.DTO.Response.BasketItem;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Queries.Basket;

public class GetAllBasketItemsQuery : IRequest<Result<List<BasketItemResponseDto>>> { }

public class GetAllBasketItemsQueryHandler(IBasketItemRepository basketItemRepository, ICurrentUserService currentUserService, ILogService logger, 
    ICacheService cacheService) : IRequestHandler<GetAllBasketItemsQuery, Result<List<BasketItemResponseDto>>>
{
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(10);
    private readonly string _cacheKey = $"{CacheKeys.AllBasketItems}_{currentUserService.GetUserEmail()}";

    public async Task<Result<List<BasketItemResponseDto>>> Handle(GetAllBasketItemsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = currentUserService.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Result<List<BasketItemResponseDto>>.Failure(ErrorMessages.UnauthorizedAction);

            var cachedItems = await cacheService.GetAsync<List<BasketItemResponseDto>>(_cacheKey);
            if (cachedItems is { Count: > 0 })
                return Result<List<BasketItemResponseDto>>.Success(cachedItems);

            var basketItems = await basketItemRepository.GetActiveItems(Guid.Parse(userId), cancellationToken);
            if (basketItems.Count == 0)
                return Result<List<BasketItemResponseDto>>.Failure(ErrorMessages.BasketItemNotFound);

            var responseItems = basketItems.Select(MapToResponseDto).ToList();
            await cacheService.SetAsync(_cacheKey, responseItems, _cacheDuration);

            return Result<List<BasketItemResponseDto>>.Success(responseItems);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ErrorMessages.UnexpectedError);
            return Result<List<BasketItemResponseDto>>.Failure(ErrorMessages.UnexpectedError);
        }
    }

    private BasketItemResponseDto MapToResponseDto(Domain.Model.BasketItem basketItem) => new BasketItemResponseDto
    {
        UserId = basketItem.UserId,
        Quantity = basketItem.Quantity,
        UnitPrice = basketItem.UnitPrice,
        ProductId = basketItem.ProductId,
        ProductName = basketItem.ProductName
    };
}