using ECommerce.Application.Abstract;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using ECommerce.Shared.Wrappers;
using MediatR;

namespace ECommerce.Application.Commands.Basket;

public class RemoveBasketItemById(Guid id) : IRequest<Result>
{
    public readonly Guid Id = id;
}

public class RemoveBasketItemByIdHandler(ICurrentUserService currentUserService, IBasketItemRepository basketItemRepository, IUnitOfWork unitOfWork, ILogService logService,
    ICacheService cache, ILockProvider lockProvider) : IRequestHandler<RemoveBasketItemById, Result>
{
    private readonly string _cacheKey = $"{CacheKeys.UserBasket}_{currentUserService.GetUserId()}";

    public async Task<Result> Handle(RemoveBasketItemById request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = currentUserService.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Result.Failure(ErrorMessages.AccountNotAuthorized);

            var basketItem = await basketItemRepository.GetById(request.Id, cancellationToken);
            if (basketItem is null)
                return Result.Failure(ErrorMessages.BasketItemNotFound);

            if (basketItem.UserId.ToString() != userId)
                return Result.Failure(ErrorMessages.UnauthorizedAction);

            using (await lockProvider.AcquireLockAsync($"basket:{userId}", cancellationToken))
            {
                basketItemRepository.Delete(basketItem);
                await cache.RemoveAsync(_cacheKey, cancellationToken);
                await unitOfWork.Commit();
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            logService.LogError(ex, ErrorMessages.UnexpectedError);
            return Result.Failure(ErrorMessages.UnexpectedError);
        }
    }
}
