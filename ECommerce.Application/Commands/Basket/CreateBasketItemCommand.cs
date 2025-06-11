using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.BasketItem;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;

namespace ECommerce.Application.Commands.Basket;
public class CreateBasketItemCommand : IRequest<Result>
{
    public required CreateBasketItemRequestDto CreateBasketItemRequestDto { get; set;}
}

public class CreateBasketItemCommandHandler : IRequestHandler<CreateBasketItemCommand, Result>
{
    private readonly IBasketItemRepository _basketItemRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IProductRepository _productRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ILoggingService _logger;
    private readonly ICacheService _cacheService;
    private const string GetAllBasketItemsCacheKey = "GetAllBasketItems";
    private const int _cacheDurationInMinutes = 30;

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

            var product = await _productRepository.GetProductById(request.CreateBasketItemRequestDto.ProductId);
            var userAccount = await _accountRepository.GetAccountByEmail(emailResult.Data);
            
            if (userAccount == null)
                return Result.Failure("Account not found");
            
            if (product == null)
                return Result.Failure("Product not found");

            if (request.CreateBasketItemRequestDto.Quantity > product.StockQuantity)
                return Result.Failure("Not enough stock");

            var basketItem = new ECommerce.Domain.Model.BasketItem
            {
                AccountId = userAccount.Id,
                ExternalId = Guid.NewGuid().ToString(),
                Quantity = request.CreateBasketItemRequestDto.Quantity,
                ProductId = product.ProductId,
                UnitPrice = product.Price,
                ProductName = product.Name,
                IsOrdered = false
            };

            await _basketItemRepository.Create(basketItem);
            await _cacheService.SetAsync(GetAllBasketItemsCacheKey, basketItem, TimeSpan.FromMinutes(_cacheDurationInMinutes));

            _logger.LogInformation("Basket item created successfully: {BasketItem}", basketItem);
            return Result.Success();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected error while creating basket item");
            return Result.Failure(exception.Message);
        }
    }
}