using ECommerce.Application.Abstract;
using ECommerce.Application.DTO.Request.BasketItem;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Commands.Basket;

public class CreateBasketItemCommand(CreateBasketItemRequestDto request) : IRequest<Result>
{
    public readonly CreateBasketItemRequestDto Model = request;
}

public class CreateBasketItemCommandHandler(IBasketItemRepository basketItemRepository, ICurrentUserService currentUserService, ILogService logger, ICacheService cacheService, 
    IAccountRepository accountRepository, IProductRepository productRepository, IUnitOfWork unitOfWork) : IRequestHandler<CreateBasketItemCommand, Result>
{
    private const int CacheDurationInMinutes = 30;

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
            logger.LogError(exception, ErrorMessages.ErrorAddingItemToBasket);
            return Result.Failure(exception.Message);
        }
    }

    private string GetValidatedUserEmail()
    {
        var email = currentUserService.GetUserEmail();
        if (string.IsNullOrEmpty(email))
            return ErrorMessages.AccountEmailNotFound;

        return email ?? ErrorMessages.AccountNotAuthorized;
    }

    private async Task<Result<(Domain.Model.Product, Domain.Model.User)>> ValidateProductAndAccount(CreateBasketItemCommand request, string email)
    {
        var product = await productRepository.GetById(request.Model.ProductId);
        if (product == null)
            return Result<(Domain.Model.Product, Domain.Model.User)>.Failure(ErrorMessages.ProductNotFound);

        var userAccount = await accountRepository.GetByEmail(email);
        if (userAccount == null)
            return Result<(Domain.Model.Product, Domain.Model.User)>.Failure(ErrorMessages.AccountNotAuthorized);

        return Result<(Domain.Model.Product, Domain.Model.User)>.Success((product, userAccount));
    }

    private Result ValidateStock(CreateBasketItemCommand request, Domain.Model.Product product)
    {
        if (request.Model.Quantity > product.StockQuantity)
            return Result.Failure(ErrorMessages.StockNotAvailable);

        return Result.Success();
    }

    private Domain.Model.BasketItem CreateBasketItem(CreateBasketItemCommand request, Domain.Model.Product product, Domain.Model.User userAccount) => new Domain.Model.BasketItem
    {
        UserId = userAccount.Id,
        ExternalId = Guid.NewGuid().ToString(),
        Quantity = request.Model.Quantity,
        ProductId = product.Id,
        UnitPrice = product.Price,
        ProductName = product.Name,
        IsPurchased = false
    };

    private async Task SaveBasketItem(Domain.Model.BasketItem basketItem)
    {
        await basketItemRepository.Create(basketItem);
        var cacheKey = $"{CacheKeys.AllBasketItems}_{currentUserService.GetUserEmail()}";
        await cacheService.SetAsync(cacheKey, basketItem, TimeSpan.FromMinutes(CacheDurationInMinutes));
        await unitOfWork.Commit();
    }
}