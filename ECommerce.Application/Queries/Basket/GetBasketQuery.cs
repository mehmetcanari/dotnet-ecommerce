using ECommerce.Application.Abstract;
using ECommerce.Application.DTO.Response.BasketItem;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Queries.Basket;

public class GetBasketQuery : IRequest<Result<List<BasketItemResponseDto>>>;

public class GetBasketQueryHandler(IBasketItemRepository basketItemRepository, ICurrentUserService currentUserService, ILogService logger, ICacheService cacheService) : IRequestHandler<GetBasketQuery, Result<List<BasketItemResponseDto>>>
{
    private readonly TimeSpan _expiration = TimeSpan.FromMinutes(15);
    private readonly string _cacheKey = $"{CacheKeys.UserBasket}_{currentUserService.GetUserId()}";

    public async Task<Result<List<BasketItemResponseDto>>> Handle(GetBasketQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = currentUserService.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Result<List<BasketItemResponseDto>>.Failure(ErrorMessages.UnauthorizedAction);

            var cacheItems = await cacheService.GetAsync<List<BasketItemResponseDto>>(_cacheKey, cancellationToken);
            if (cacheItems is { Count: > 0 })
                return Result<List<BasketItemResponseDto>>.Success(cacheItems);

            var basketItems = await basketItemRepository.GetActiveItems(Guid.Parse(userId), cancellationToken);
            if (basketItems.Count == 0)
                return Result<List<BasketItemResponseDto>>.Failure(ErrorMessages.BasketItemNotFound);

            var responseItems = basketItems.Select(MapToResponseDto).ToList();
            await cacheService.SetAsync(_cacheKey, responseItems, CacheExpirationType.Sliding, _expiration, cancellationToken);

            return Result<List<BasketItemResponseDto>>.Success(responseItems);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ErrorMessages.UnexpectedError);
            return Result<List<BasketItemResponseDto>>.Failure(ErrorMessages.UnexpectedError);
        }
    }

    private BasketItemResponseDto MapToResponseDto(BasketItem basketItem) => new()
    {
        UserId = basketItem.UserId,
        Quantity = basketItem.Quantity,
        UnitPrice = basketItem.UnitPrice,
        ProductId = basketItem.ProductId,
        ProductName = basketItem.ProductName
    };
}