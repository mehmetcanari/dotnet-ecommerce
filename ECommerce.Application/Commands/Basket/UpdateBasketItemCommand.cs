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

public class UpdateBasketItemCommandHandler(IBasketItemRepository basketItemRepository, ICurrentUserService currentUserService, ILogService logger, IUserRepository userRepository, 
    IProductRepository productRepository, IUnitOfWork unitOfWork, ICacheService cacheService) : IRequestHandler<UpdateBasketItemCommand, Result>
{
    private const int CacheDurationInMinutes = 30;

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

            await UpdateBasketItem(basketItem, product, request);

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
        basketItem.ProductId = request.Model.ProductId;
        basketItem.ProductName = product.Name;
        basketItem.UnitPrice = product.Price;
        basketItem.IsPurchased = false;

        basketItemRepository.Update(basketItem);
        var cacheKey = $"{CacheKeys.AllBasketItems}_{currentUserService.GetUserEmail()}";
        await cacheService.SetAsync(cacheKey, basketItem, TimeSpan.FromMinutes(CacheDurationInMinutes));
        await unitOfWork.Commit();
    }
}