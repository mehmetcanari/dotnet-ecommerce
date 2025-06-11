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
    private const string GetAllBasketItemsCacheKey = "GetAllBasketItems";
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

    public async Task<Result<List<BasketItemResponseDto>>> 
    Handle(GetAllBasketItemsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var emailResult = _currentUserService.GetCurrentUserEmail();
            if (emailResult is { IsSuccess: false, Error: not null })
            {
                _logger.LogWarning("Failed to get current user email: {Error}", emailResult.Error);
                return Result<List<BasketItemResponseDto>>.Failure(emailResult.Error);
            }

            TimeSpan cacheDuration = TimeSpan.FromMinutes(_cacheDurationInMinutes);
            var cachedItems = await _cacheService.GetAsync<List<BasketItemResponseDto>>(GetAllBasketItemsCacheKey);
            if (cachedItems != null)
            {
                _logger.LogInformation("Basket items fetched from cache");
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

            var nonOrderedBasketItems = await _basketItemRepository.GetNonOrderedBasketItems(account);
            if (nonOrderedBasketItems.Count == 0)
            {
                return Result<List<BasketItemResponseDto>>.Failure("No basket items found.");
            }

            var clientResponseBasketItems = nonOrderedBasketItems
            .Select(basketItem => new BasketItemResponseDto
            {
                AccountId = basketItem.AccountId,
                Quantity = basketItem.Quantity,
                UnitPrice = basketItem.UnitPrice,
                ProductId = basketItem.ProductId,
                ProductName = basketItem.ProductName
            }).ToList();

            await _cacheService.SetAsync(GetAllBasketItemsCacheKey, clientResponseBasketItems, cacheDuration);

            return Result<List<BasketItemResponseDto>>.Success(clientResponseBasketItems);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while fetching all basket items");
            return Result<List<BasketItemResponseDto>>.Failure("An unexpected error occurred");
        }
    }
}