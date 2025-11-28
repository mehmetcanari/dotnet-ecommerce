using ECommerce.Application.Abstract;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using ECommerce.Shared.Enum;
using ECommerce.Shared.Wrappers;
using MediatR;

namespace ECommerce.Application.Commands.Order;

public class CancelOrderCommand(Guid id) : IRequest<Result>
{
    public readonly Guid Id = id;
}

public class CancelOrderCommandHandler(ICurrentUserService currentUserService, ILogService logger, IOrderRepository orderRepository, IUnitOfWork unitOfWork,
    ICacheService cache, ILockProvider lockProvider) : IRequestHandler<CancelOrderCommand, Result>
{
    public async Task<Result> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = currentUserService.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Result.Failure(ErrorMessages.UnauthorizedAction);

            var pendingOrder = await orderRepository.GetPendingOrderById(Guid.Parse(userId), request.Id, cancellationToken);
            if (pendingOrder is null)
                return Result.Failure(ErrorMessages.NoPendingOrders);

            using (await lockProvider.AcquireLockAsync($"order:{pendingOrder.Id}", cancellationToken))
            {
                await cache.RemoveAsync($"{CacheKeys.UserOrders}_{userId}", cancellationToken);
                pendingOrder.UpdateStatus(OrderStatus.Cancelled);
                await unitOfWork.Commit();
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.UnexpectedError);
            return Result.Failure(ex.Message);
        }
    }
}