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

    public CreateBasketItemCommandHandler(
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

    public async Task<Result> Handle(CreateBasketItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var emailResult = await GetValidatedUserEmail();
            if (emailResult.IsFailure)
                return Result.Failure(emailResult.Error);

            var validationResult = await ValidateProductAndAccount(request, emailResult.Data);
            if (validationResult.IsFailure)
                return Result.Failure(emailResult.Error);

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
            _logger.LogError(exception, "Unexpected error while creating basket item");
            return Result.Failure(exception.Message);
        }
    }

    private async Task<Result<string>> GetValidatedUserEmail()
    {
        var emailResult = _currentUserService.GetUserEmail();
        if (emailResult is { IsSuccess: false, Error: not null })
        {
            _logger.LogWarning("Failed to get current user email: {Error}", emailResult.Error);
            return Result<string>.Failure(emailResult.Error);
        }

        if (emailResult.Data == null)
        {
            _logger.LogWarning("User email is null");
            return Result<string>.Failure("User email is null");
        }

        return Result<string>.Success(emailResult.Data);
    }

    private async Task<Result<(Domain.Model.Product, Domain.Model.Account)>> ValidateProductAndAccount(
        CreateBasketItemCommand request, string email)
    {
        var product = await _productRepository.GetProductById(request.CreateBasketItemRequestDto.ProductId);
        if (product == null)
            return Result<(Domain.Model.Product, Domain.Model.Account)>.Failure("Product not found");

        var userAccount = await _accountRepository.GetAccountByEmail(email);
        if (userAccount == null)
            return Result<(Domain.Model.Product, Domain.Model.Account)>.Failure("Account not found");

        return Result<(Domain.Model.Product, Domain.Model.Account)>.Success((product, userAccount));
    }

    private Result ValidateStock(CreateBasketItemCommand request, Domain.Model.Product product)
    {
        if (request.CreateBasketItemRequestDto.Quantity > product.StockQuantity)
            return Result.Failure("Not enough stock");

        return Result.Success();
    }

    private Domain.Model.BasketItem CreateBasketItem(
        CreateBasketItemCommand request,
        Domain.Model.Product product,
        Domain.Model.Account userAccount)
    {
        return new Domain.Model.BasketItem
        {
            AccountId = userAccount.Id,
            ExternalId = Guid.NewGuid().ToString(),
            Quantity = request.CreateBasketItemRequestDto.Quantity,
            ProductId = product.ProductId,
            UnitPrice = product.Price,
            ProductName = product.Name,
            IsOrdered = false
        };
    }

    private async Task SaveBasketItem(Domain.Model.BasketItem basketItem)
    {
        await _basketItemRepository.Create(basketItem);
        var cacheKey = $"{CacheKeys.AllBasketItems}_{_currentUserService.GetUserEmail().Data}";
        await _cacheService.SetAsync(cacheKey, basketItem, TimeSpan.FromMinutes(CacheDurationInMinutes));
        _logger.LogInformation("Basket item created successfully: {BasketItem}", basketItem);
    }
}