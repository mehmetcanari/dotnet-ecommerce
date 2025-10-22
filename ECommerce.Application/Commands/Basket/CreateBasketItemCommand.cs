using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.BasketItem;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Commands.Basket;

public class CreateBasketItemCommand : IRequest<Result>
{
    public required CreateBasketItemRequestDto CreateBasketItemRequestDto { get; set; }
}

public class CreateBasketItemCommandHandler : IRequestHandler<CreateBasketItemCommand, Result>
{
    private readonly IBasketItemRepository _basketItemRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IProductRepository _productRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ILoggingService _logger;
    private readonly ICacheService _cacheService;
    private const int CacheDurationInMinutes = 30;

    public CreateBasketItemCommandHandler(IBasketItemRepository basketItemRepository, ICurrentUserService currentUserService, ILoggingService logger, ICacheService cacheService, 
        IAccountRepository accountRepository, IProductRepository productRepository)
    {
        _basketItemRepository = basketItemRepository;
        _currentUserService = currentUserService;
        _logger = logger;
        _cacheService = cacheService;
        _accountRepository = accountRepository;
        _productRepository = productRepository;
    }

    public async Task<Result> Handle(CreateBasketItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var emailResult = GetValidatedUserEmail();
            var validationResult = await ValidateProductAndAccount(request, emailResult);
            if (validationResult.IsFailure)
                return Result.Failure(ErrorMessages.AccountNotFound);

            var (product, userAccount) = validationResult.Data;

            var stockValidationResult = ValidateStock(request, product);
            if (stockValidationResult.IsFailure)
                return stockValidationResult;

            var basketItem = CreateBasketItem(request, product, userAccount);
            await SaveBasketItem(basketItem);

            return Result.Success();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, ErrorMessages.ErrorAddingItemToBasket);
            return Result.Failure(exception.Message);
        }
    }

    private string GetValidatedUserEmail()
    {
        var email = _currentUserService.GetUserEmail();
        if (string.IsNullOrEmpty(email))
            return ErrorMessages.AccountEmailNotFound;

        return email ?? string.Empty;
    }

    private async Task<Result<(Domain.Model.Product, Domain.Model.Account)>> ValidateProductAndAccount(CreateBasketItemCommand request, string email)
    {
        var product = await _productRepository.GetProductById(request.CreateBasketItemRequestDto.ProductId);
        if (product == null)
            return Result<(Domain.Model.Product, Domain.Model.Account)>.Failure(ErrorMessages.ProductNotFound);

        var userAccount = await _accountRepository.GetAccountByEmail(email);
        if (userAccount == null)
            return Result<(Domain.Model.Product, Domain.Model.Account)>.Failure(ErrorMessages.AccountNotFound);

        return Result<(Domain.Model.Product, Domain.Model.Account)>.Success((product, userAccount));
    }

    private Result ValidateStock(CreateBasketItemCommand request, Domain.Model.Product product)
    {
        if (request.CreateBasketItemRequestDto.Quantity > product.StockQuantity)
            return Result.Failure(ErrorMessages.StockNotAvailable);

        return Result.Success();
    }

    private Domain.Model.BasketItem CreateBasketItem(CreateBasketItemCommand request, Domain.Model.Product product, Domain.Model.Account userAccount) => new Domain.Model.BasketItem
    {
        AccountId = userAccount.Id,
        ExternalId = Guid.NewGuid().ToString(),
        Quantity = request.CreateBasketItemRequestDto.Quantity,
        ProductId = product.ProductId,
        UnitPrice = product.Price,
        ProductName = product.Name,
        IsOrdered = false
    };

    private async Task SaveBasketItem(Domain.Model.BasketItem basketItem)
    {
        await _basketItemRepository.Create(basketItem);
        var cacheKey = $"{CacheKeys.AllBasketItems}_{_currentUserService.GetUserEmail()}";
        await _cacheService.SetAsync(cacheKey, basketItem, TimeSpan.FromMinutes(CacheDurationInMinutes));
    }
}