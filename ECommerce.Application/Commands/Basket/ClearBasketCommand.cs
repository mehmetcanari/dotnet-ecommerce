using ECommerce.Application.Abstract;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Commands.Basket;

public class ClearBasketCommand : IRequest<Result>;

public class ClearBasketCommandHandler(IBasketItemRepository basketItemRepository, ICurrentUserService currentUserService, ILogService logger, IUnitOfWork unitOfWork,
    ICacheService cache, ILockProvider lockProvider) : IRequestHandler<ClearBasketCommand, Result>
{
    private readonly string _cacheKey = $"{CacheKeys.UserBasket}_{currentUserService.GetUserId()}";

    public async Task<Result> Handle(ClearBasketCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = currentUserService.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Result.Failure(ErrorMessages.AccountEmailNotFound);

            var basketItems = await basketItemRepository.GetActiveItems(Guid.Parse(userId), cancellationToken);
            if (basketItems.Count == 0)
                return Result.Failure(ErrorMessages.BasketItemNotFound);

            using (await lockProvider.AcquireLockAsync($"basket:{userId}", cancellationToken))
            {
                foreach (var basketItem in basketItems)
                {
                    basketItemRepository.Delete(basketItem);
                }


                await cache.RemoveAsync(_cacheKey, cancellationToken);
                await unitOfWork.Commit();
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.UnexpectedError);
            return Result.Failure(ErrorMessages.UnexpectedError);
        }
    }
}