using ECommerce.Application.Abstract;
using ECommerce.Application.DTO.Response.BasketItem;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Queries.Basket;

public class GetAllBasketItemsQuery : IRequest<Result<List<BasketItemResponseDto>>> { }

public class GetAllBasketItemsQueryHandler(IBasketItemRepository basketItemRepository, ICurrentUserService currentUserService, ILogService logger, 
    ICacheService cacheService, IAccountRepository accountRepository) : IRequestHandler<GetAllBasketItemsQuery, Result<List<BasketItemResponseDto>>>
{
    private const int CacheExpirationInMinutes = 10;

    public async Task<Result<List<BasketItemResponseDto>>> Handle(GetAllBasketItemsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var email = currentUserService.GetUserEmail();
            if (string.IsNullOrEmpty(email))
                return Result<List<BasketItemResponseDto>>.Failure(ErrorMessages.AccountEmailNotFound);

            var cachedItems = await GetCachedBasketItems();
            if (cachedItems != null)
                return Result<List<BasketItemResponseDto>>.Success(cachedItems);

            var account = await accountRepository.GetByEmail(email, cancellationToken);
            if (account == null)
                return Result<List<BasketItemResponseDto>>.Failure(ErrorMessages.AccountNotFound);

            var basketItems = await basketItemRepository.GetUnorderedItems(account, cancellationToken);
            if (basketItems.Count == 0)
                return Result<List<BasketItemResponseDto>>.Failure(ErrorMessages.BasketItemNotFound);

            var responseItems = basketItems.Select(MapToResponseDto).ToList();
            await CacheBasketItems(responseItems);

            return Result<List<BasketItemResponseDto>>.Success(responseItems);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ErrorMessages.UnexpectedError);
            return Result<List<BasketItemResponseDto>>.Failure(ErrorMessages.UnexpectedError);
        }
    }

    private async Task<List<BasketItemResponseDto>?> GetCachedBasketItems()
    {
        var cacheKey = $"{CacheKeys.AllBasketItems}_{currentUserService.GetUserEmail()}";
        var cachedItems = await cacheService.GetAsync<List<BasketItemResponseDto>>(cacheKey);

        return cachedItems;
    }

    private async Task CacheBasketItems(List<BasketItemResponseDto> items)
    {
        var cacheKey = $"{CacheKeys.AllBasketItems}_{currentUserService.GetUserEmail()}";
        TimeSpan cacheDuration = TimeSpan.FromMinutes(CacheExpirationInMinutes);
        await cacheService.SetAsync(cacheKey, items, cacheDuration);
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