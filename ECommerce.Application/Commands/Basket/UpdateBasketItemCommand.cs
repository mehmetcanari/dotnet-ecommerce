using ECommerce.Application.Abstract;
using ECommerce.Application.DTO.Request.BasketItem;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Commands.Basket;

public class UpdateBasketItemCommand(UpdateBasketItemRequestDto request) : IRequest<Result>
{
    public readonly UpdateBasketItemRequestDto Model = request;
}

public class UpdateBasketItemCommandHandler(IBasketItemRepository basketItemRepository, ICurrentUserService currentUserService, ILogService logger, 
    IProductRepository productRepository, IUnitOfWork unitOfWork, ICacheService cache, ILockProvider lockProvider) : IRequestHandler<UpdateBasketItemCommand, Result>
{
    private readonly string _cacheKey = $"{CacheKeys.UserBasket}_{currentUserService.GetUserId()}";

    public async Task<Result> Handle(UpdateBasketItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = currentUserService.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Result.Failure(ErrorMessages.AccountNotAuthorized);

            var basketItem = await basketItemRepository.GetById(request.Model.Id, cancellationToken);
            if (basketItem is null)
                return Result.Failure(ErrorMessages.BasketItemNotFound);

            var product = await productRepository.GetById(request.Model.ProductId, cancellationToken);
            if (product is null)
                return Result.Failure(ErrorMessages.ProductNotFound);

            if (request.Model.Quantity > product.StockQuantity)
                return Result.Failure(ErrorMessages.StockNotAvailable);

            using (await lockProvider.AcquireLockAsync($"basket:{userId}", cancellationToken))
            {
                await UpdateBasketItem(basketItem, product, request);
                await cache.RemoveAsync(_cacheKey, cancellationToken);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.UnexpectedError);
            return Result.Failure(ErrorMessages.UnexpectedError);
        }
    }

    private async Task UpdateBasketItem(Domain.Model.BasketItem basketItem, Domain.Model.Product product, UpdateBasketItemCommand request)
    {
        basketItem.Quantity = request.Model.Quantity;
        basketItem.ProductId = product.Id;
        basketItem.ProductName = product.Name;
        basketItem.UnitPrice = product.Price;

        basketItemRepository.Update(basketItem);
        await unitOfWork.Commit();
    }
}