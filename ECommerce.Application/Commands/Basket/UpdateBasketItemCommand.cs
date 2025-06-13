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
            var emailResult = await GetValidatedUserEmail();
            if (emailResult.IsFailure)
                return Result.Failure(emailResult.Error);

            var accountResult = await ValidateAndGetAccount(emailResult.Data);
            if (accountResult.IsFailure)
                return Result.Failure(accountResult.Error);

            var basketItemResult = await ValidateAndGetBasketItem(request, accountResult.Data);
            if (basketItemResult.IsFailure)
                return Result.Failure(basketItemResult.Error);

            var productResult = await ValidateAndGetProduct(request);
            if (productResult.IsFailure)
                return Result.Failure(productResult.Error);

            var stockValidationResult = ValidateStock(request, productResult.Data);
            if (stockValidationResult.IsFailure)
                return stockValidationResult;

            await UpdateBasketItem(basketItemResult.Data, productResult.Data, request);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating the basket item");
            return Result.Failure("An error occurred while updating the basket item");
        }
    }

    private async Task<Result<string>> GetValidatedUserEmail()
    {
        var emailResult = await Task.FromResult(_currentUserService.GetCurrentUserEmail());
        if (emailResult.IsFailure)
        {
            _logger.LogWarning("Failed to get current user email: {Error}", emailResult.Error);
            return Result<string>.Failure(emailResult.Error);
        }

        if (string.IsNullOrEmpty(emailResult.Data))
        {
            _logger.LogWarning("User email is null or empty");
            return Result<string>.Failure("Email is not available");
        }

        return Result<string>.Success(emailResult.Data);
    }

    private async Task<Result<Domain.Model.Account>> ValidateAndGetAccount(string email)
    {
        var account = await _accountRepository.GetAccountByEmail(email);
        if (account == null)
        {
            _logger.LogWarning("Account not found for email: {Email}", email);
            return Result<Domain.Model.Account>.Failure("Account not found");
        }

        return Result<Domain.Model.Account>.Success(account);
    }

    private async Task<Result<Domain.Model.BasketItem>> ValidateAndGetBasketItem(
        UpdateBasketItemCommand request,
        Domain.Model.Account account)
    {
        var basketItem = await _basketItemRepository.GetSpecificAccountBasketItemWithId(
            request.UpdateBasketItemRequestDto.BasketItemId, account);

        if (basketItem == null)
        {
            _logger.LogWarning("Basket item not found with ID: {BasketItemId} for account: {AccountId}",
                request.UpdateBasketItemRequestDto.BasketItemId, account.Id);
            return Result<Domain.Model.BasketItem>.Failure("Basket item not found");
        }

        return Result<Domain.Model.BasketItem>.Success(basketItem);
    }

    private async Task<Result<Domain.Model.Product>> ValidateAndGetProduct(UpdateBasketItemCommand request)
    {
        var product = await _productRepository.GetProductById(request.UpdateBasketItemRequestDto.ProductId);
        if (product == null)
        {
            _logger.LogWarning("Product not found with ID: {ProductId}", request.UpdateBasketItemRequestDto.ProductId);
            return Result<Domain.Model.Product>.Failure("Product not found");
        }

        return Result<Domain.Model.Product>.Success(product);
    }

    private Result ValidateStock(UpdateBasketItemCommand request, Domain.Model.Product product)
    {
        if (request.UpdateBasketItemRequestDto.Quantity > product.StockQuantity)
        {
            _logger.LogWarning("Insufficient stock for product {ProductId}. Requested: {Quantity}, Available: {StockQuantity}",
                product.ProductId, request.UpdateBasketItemRequestDto.Quantity, product.StockQuantity);
            return Result.Failure("Not enough stock");
        }

        return Result.Success();
    }

    private async Task UpdateBasketItem(
        Domain.Model.BasketItem basketItem,
        Domain.Model.Product product,
        UpdateBasketItemCommand request)
    {
        basketItem.Quantity = request.UpdateBasketItemRequestDto.Quantity;
        basketItem.ProductId = request.UpdateBasketItemRequestDto.ProductId;
        basketItem.ProductName = product.Name;
        basketItem.UnitPrice = product.Price;
        basketItem.IsOrdered = false;

        _basketItemRepository.Update(basketItem);
        await ClearBasketItemsCacheAsync();
        
        _logger.LogInformation("Basket item updated successfully: {BasketItemId}", basketItem.BasketItemId);
    }

    private async Task ClearBasketItemsCacheAsync()
    {
        await _cacheService.RemoveAsync(GetAllBasketItemsCacheKey);
    }
}