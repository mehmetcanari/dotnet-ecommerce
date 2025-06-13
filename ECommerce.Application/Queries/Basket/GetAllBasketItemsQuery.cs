using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Response.BasketItem;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
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
    private const string GetAllBasketItemsCacheKeyPrefix = "GetAllBasketItems";
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
            var emailResult = _currentUserService.GetCurrentUserEmail();
            if (emailResult is { IsSuccess: false, Error: not null })
            {
                _logger.LogWarning("Failed to get current user email: {Error}", emailResult.Error);
                return Result<List<BasketItemResponseDto>>.Failure(emailResult.Error);
            }

            var cachedItems = await GetCachedBasketItems();
            if (cachedItems != null)
            {
                return Result<List<BasketItemResponseDto>>.Success(cachedItems);
            }

            if (emailResult.Data == null)
            {
                _logger.LogWarning("User email is null");
                return Result<List<BasketItemResponseDto>>.Failure("Email is not available");
            }

            var account = await _accountRepository.GetAccountByEmail(emailResult.Data);
            if (account == null)
            {
                return Result<List<BasketItemResponseDto>>.Failure("Account not found");
            }

            var basketItems = await GetBasketItems(account);
            if (basketItems.Count == 0)
            {
                return Result<List<BasketItemResponseDto>>.Failure("No basket items found.");
            }

            var responseItems = basketItems.Select(MapToResponseDto).ToList();
            await CacheBasketItems(responseItems);

            return Result<List<BasketItemResponseDto>>.Success(responseItems);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while fetching all basket items");
            return Result<List<BasketItemResponseDto>>.Failure("An unexpected error occurred");
        }
    }

    private async Task<List<BasketItemResponseDto>?> GetCachedBasketItems()
    {
        var cacheKey = $"{GetAllBasketItemsCacheKeyPrefix}_{_currentUserService.GetCurrentUserEmail().Data}";
        var cachedItems = await _cacheService.GetAsync<List<BasketItemResponseDto>>(cacheKey);
        if (cachedItems != null)
        {
            _logger.LogInformation("Basket items fetched from cache for user: {Email}", _currentUserService.GetCurrentUserEmail().Data);
        }
        return cachedItems;
    }

    private async Task<List<Domain.Model.BasketItem>> GetBasketItems(Domain.Model.Account account)
    {
        return await _basketItemRepository.GetNonOrderedBasketItems(account);
    }

    private async Task CacheBasketItems(List<BasketItemResponseDto> items)
    {
        var cacheKey = $"{GetAllBasketItemsCacheKeyPrefix}_{_currentUserService.GetCurrentUserEmail().Data}";
        TimeSpan cacheDuration = TimeSpan.FromMinutes(_cacheDurationInMinutes);
        await _cacheService.SetAsync(cacheKey, items, cacheDuration);
    }

    private static BasketItemResponseDto MapToResponseDto(Domain.Model.BasketItem basketItem)
    {
        return new BasketItemResponseDto
        {
            AccountId = basketItem.AccountId,
            Quantity = basketItem.Quantity,
            UnitPrice = basketItem.UnitPrice,
            ProductId = basketItem.ProductId,
            ProductName = basketItem.ProductName
        };
    }
}