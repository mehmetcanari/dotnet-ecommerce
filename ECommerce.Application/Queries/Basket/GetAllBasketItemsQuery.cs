using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Response.BasketItem;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Queries.Basket;

public class GetAllBasketItemsQuery : IRequest<Result<List<BasketItemResponseDto>>> { }

public class GetAllBasketItemsQueryHandler : IRequestHandler<GetAllBasketItemsQuery, Result<List<BasketItemResponseDto>>>
{
    private readonly IBasketItemRepository _basketItemRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILoggingService _logger;
    private readonly ICacheService _cacheService;
    private const int _cacheDurationInMinutes = 10;

    public GetAllBasketItemsQueryHandler(
        IBasketItemRepository basketItemRepository,
        ICurrentUserService currentUserService,
        ILoggingService logger,
        ICacheService cacheService,
        IAccountRepository accountRepository)
    {
        _basketItemRepository = basketItemRepository;
        _currentUserService = currentUserService;
        _logger = logger;
        _cacheService = cacheService;
        _accountRepository = accountRepository;
    }

    public async Task<Result<List<BasketItemResponseDto>>> Handle(GetAllBasketItemsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var email = _currentUserService.GetUserEmail();
            if (email is null)
                return Result<List<BasketItemResponseDto>>.Failure(ErrorMessages.AccountEmailNotFound);

            var cachedItems = await GetCachedBasketItems();
            if (cachedItems != null)
                return Result<List<BasketItemResponseDto>>.Success(cachedItems);

            var account = await _accountRepository.GetAccountByEmail(email);
            if (account == null)
                return Result<List<BasketItemResponseDto>>.Failure(ErrorMessages.AccountNotFound);

            var basketItems = await GetBasketItems(account);
            if (basketItems.Count == 0)
                return Result<List<BasketItemResponseDto>>.Failure(ErrorMessages.BasketItemNotFound);

            var responseItems = basketItems.Select(MapToResponseDto).ToList();
            await CacheBasketItems(responseItems);

            return Result<List<BasketItemResponseDto>>.Success(responseItems);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, ErrorMessages.UnexpectedError);
            return Result<List<BasketItemResponseDto>>.Failure(ErrorMessages.UnexpectedError);
        }
    }

    private async Task<List<BasketItemResponseDto>?> GetCachedBasketItems()
    {
        var cacheKey = $"{CacheKeys.AllBasketItems}_{_currentUserService.GetUserEmail()}";
        var cachedItems = await _cacheService.GetAsync<List<BasketItemResponseDto>>(cacheKey);

        return cachedItems;
    }

    private async Task<List<Domain.Model.BasketItem>> GetBasketItems(Domain.Model.User account)
    {
        return await _basketItemRepository.GetNonOrderedBasketItems(account);
    }

    private async Task CacheBasketItems(List<BasketItemResponseDto> items)
    {
        var cacheKey = $"{CacheKeys.AllBasketItems}_{_currentUserService.GetUserEmail()}";
        TimeSpan cacheDuration = TimeSpan.FromMinutes(_cacheDurationInMinutes);
        await _cacheService.SetAsync(cacheKey, items, cacheDuration);
    }

    private static BasketItemResponseDto MapToResponseDto(Domain.Model.BasketItem basketItem) => new BasketItemResponseDto
    {
        UserId = basketItem.UserId,
        Quantity = basketItem.Quantity,
        UnitPrice = basketItem.UnitPrice,
        ProductId = basketItem.ProductId,
        ProductName = basketItem.ProductName
    };
}