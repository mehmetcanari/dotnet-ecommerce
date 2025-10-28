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
IProductRepository productRepository, IUnitOfWork unitOfWork) : IRequestHandler<CreateBasketItemCommand, Result>
{
    private readonly string _cacheKey = $"{CacheKeys.AllBasketItems}_{currentUserService.GetUserEmail()}";
    private const int CacheDurationInMinutes = 30;

    public async Task<Result> Handle(CreateBasketItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = currentUserService.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Result.Failure(ErrorMessages.AccountNotAuthorized);

            var product = await productRepository.GetById(request.Model.ProductId, cancellationToken);
            if (product is null)
                return Result.Failure(ErrorMessages.ProductNotFound);

            if (request.Model.Quantity > product.StockQuantity)
                return Result.Failure(ErrorMessages.StockNotAvailable);

            var basketItem = CreateBasketItem(request, product, userId);

            await basketItemRepository.Create(basketItem, cancellationToken);
            await cacheService.SetAsync(_cacheKey, basketItem, TimeSpan.FromMinutes(CacheDurationInMinutes));
            await unitOfWork.Commit();

            return Result.Success();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ErrorMessages.ErrorAddingItemToBasket);
            return Result.Failure(exception.Message);
        }
    }

    private Domain.Model.BasketItem CreateBasketItem(CreateBasketItemCommand request, Domain.Model.Product product, string userId) => new Domain.Model.BasketItem
    {
        UserId = Guid.Parse(userId),
        ExternalId = Guid.NewGuid().ToString(),
        Quantity = request.Model.Quantity,
        ProductId = product.Id,
        UnitPrice = product.Price,
        ProductName = product.Name,
        IsPurchased = false
    };
}