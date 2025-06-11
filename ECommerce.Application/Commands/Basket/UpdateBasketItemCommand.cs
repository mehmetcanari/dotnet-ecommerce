using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.BasketItem;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;

namespace ECommerce.Application.Commands.Basket;

public class UpdateBasketItemCommand : IRequest<Result>
{
    public required UpdateBasketItemRequestDto UpdateBasketItemRequestDto { get; set; }
}

public class UpdateBasketItemCommandHandler : IRequestHandler<UpdateBasketItemCommand, Result>
{
    private readonly IBasketItemRepository _basketItemRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IProductRepository _productRepository;
    private readonly ILoggingService _logger;
    private readonly ICacheService _cacheService;
    private const string GetAllBasketItemsCacheKey = "GetAllBasketItems";

    public UpdateBasketItemCommandHandler(
        IBasketItemRepository basketItemRepository,
        ICurrentUserService currentUserService,
        ILoggingService logger,
        ICacheService cacheService,
        IAccountRepository accountRepository,
        IProductRepository productRepository)
    {
        _basketItemRepository = basketItemRepository;
        _currentUserService = currentUserService;
        _logger = logger;
        _cacheService = cacheService;
        _accountRepository = accountRepository;
        _productRepository = productRepository;
    }

    public async Task<Result> Handle(UpdateBasketItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var emailResult = _currentUserService.GetCurrentUserEmail();
            if (emailResult is { IsSuccess: false, Error: not null })
            {
                _logger.LogWarning("Failed to get current user email: {Error}", emailResult.Error);
                return Result.Failure(emailResult.Error);
            }
            if (emailResult.Data == null)
            {
                _logger.LogWarning("User email is null");
                return Result.Failure("Email is not available");
            }
            
            var account = await _accountRepository.GetAccountByEmail(emailResult.Data);
            if (account == null)
                return Result.Failure("Account not found");
            
            var basketItem = await _basketItemRepository.GetSpecificAccountBasketItemWithId(request.UpdateBasketItemRequestDto.BasketItemId, account);
            if (basketItem == null)
                return Result.Failure("Basket item not found");

            var product = await _productRepository.GetProductById(request.UpdateBasketItemRequestDto.ProductId);
            if (product == null)
                return Result.Failure("Product not found");

            if (product.StockQuantity < request.UpdateBasketItemRequestDto.Quantity)
            {
                return Result.Failure("Not enough stock");
            }

            basketItem.Quantity = request.UpdateBasketItemRequestDto.Quantity;
            basketItem.ProductId = request.UpdateBasketItemRequestDto.ProductId;
            basketItem.ProductName = product.Name;
            basketItem.UnitPrice = product.Price;
            basketItem.IsOrdered = false;

            _basketItemRepository.Update(basketItem);
            _logger.LogInformation("Basket item updated successfully: {BasketItemId}", basketItem.BasketItemId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating the basket item");
            return Result.Failure("An error occurred while updating the basket item");
        }
    }

    private async Task ClearBasketItemsCacheAsync()
    {
        await _cacheService.RemoveAsync(GetAllBasketItemsCacheKey);
    }
}