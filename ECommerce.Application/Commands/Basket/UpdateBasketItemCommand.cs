using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.BasketItem;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
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
    private readonly IBasketItemService _basketItemService;

    public UpdateBasketItemCommandHandler(IBasketItemRepository basketItemRepository, ICurrentUserService currentUserService, ILoggingService logger, ICacheService cacheService,
        IAccountRepository accountRepository, IProductRepository productRepository, IBasketItemService basketItemService)
    {
        _basketItemRepository = basketItemRepository;
        _currentUserService = currentUserService;
        _logger = logger;
        _accountRepository = accountRepository;
        _productRepository = productRepository;
        _basketItemService = basketItemService;
    }

    public async Task<Result> Handle(UpdateBasketItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var emailResult = GetValidatedUserEmail();

            var accountResult = await ValidateAndGetAccount(emailResult);
            if (accountResult.IsFailure && accountResult.Error is not null)
                return Result.Failure(accountResult.Error);

            if (accountResult.Data == null)
                return Result.Failure(ErrorMessages.AccountNotFound);

            var basketItemResult = await ValidateAndGetBasketItem(request, accountResult.Data);
            if (basketItemResult.IsFailure && basketItemResult.Error is not null)
                return Result.Failure(basketItemResult.Error);

            var productResult = await ValidateAndGetProduct(request);
            if (productResult.IsFailure && productResult.Error is not null)
                return Result.Failure(productResult.Error);

            if (productResult.Data == null)
                return Result.Failure(ErrorMessages.ProductNotFound);

            var stockValidationResult = ValidateStock(request, productResult.Data);
            if (stockValidationResult.IsFailure)
                return Result.Failure(ErrorMessages.StockNotAvailable);

            if (basketItemResult.Data == null)
                return Result.Failure(ErrorMessages.BasketItemNotFound);

            await UpdateBasketItem(basketItemResult.Data, productResult.Data, request);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.UnexpectedError);
            return Result.Failure(ErrorMessages.UnexpectedError);
        }
    }

    private string GetValidatedUserEmail()
    {
        var email = _currentUserService.GetUserEmail();
        if (string.IsNullOrEmpty(email))
            return string.Empty;

        return email;
    }

    private async Task<Result<Domain.Model.User>> ValidateAndGetAccount(string email)
    {
        var account = await _accountRepository.GetAccountByEmail(email);
        if (account == null)
        {
            return Result<Domain.Model.User>.Failure(ErrorMessages.AccountNotFound);
        }

        return Result<Domain.Model.User>.Success(account);
    }

    private async Task<Result<Domain.Model.BasketItem>> ValidateAndGetBasketItem(UpdateBasketItemCommand request, Domain.Model.User account)
    {
        var basketItem = await _basketItemRepository.GetSpecificAccountBasketItemWithId(request.UpdateBasketItemRequestDto.BasketItemId, account);

        if (basketItem == null)
        {
            return Result<Domain.Model.BasketItem>.Failure(ErrorMessages.BasketItemNotFound);
        }

        return Result<Domain.Model.BasketItem>.Success(basketItem);
    }

    private async Task<Result<Domain.Model.Product>> ValidateAndGetProduct(UpdateBasketItemCommand request)
    {
        var product = await _productRepository.GetProductById(request.UpdateBasketItemRequestDto.ProductId);
        if (product == null)
        {
            return Result<Domain.Model.Product>.Failure(ErrorMessages.ProductNotFound);
        }

        return Result<Domain.Model.Product>.Success(product);
    }

    private Result ValidateStock(UpdateBasketItemCommand request, Domain.Model.Product product)
    {
        if (request.UpdateBasketItemRequestDto.Quantity > product.StockQuantity)
        {
            return Result.Failure(ErrorMessages.StockNotAvailable);
        }

        return Result.Success();
    }

    private async Task UpdateBasketItem(Domain.Model.BasketItem basketItem, Domain.Model.Product product, UpdateBasketItemCommand request)
    {
        basketItem.Quantity = request.UpdateBasketItemRequestDto.Quantity;
        basketItem.ProductId = request.UpdateBasketItemRequestDto.ProductId;
        basketItem.ProductName = product.Name;
        basketItem.UnitPrice = product.Price;
        basketItem.IsOrdered = false;

        _basketItemRepository.Update(basketItem);
        await _basketItemService.ClearBasketItemsCacheAsync();
    }
}