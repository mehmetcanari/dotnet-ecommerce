using ECommerce.Application.Abstract;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Commands.Order;

public class CancelOrderCommand(Guid id) : IRequest<Result>
{
    public readonly Guid Id = id;
}

public class CancelOrderCommandHandler(ICurrentUserService currentUserService, ILogService logger, IOrderRepository orderRepository, IUnitOfWork unitOfWork, ICacheService cache) : IRequestHandler<CancelOrderCommand, Result>
{
    public async Task<Result> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = currentUserService.GetUserId();
            if(string.IsNullOrEmpty(userId))
                return Result.Failure(ErrorMessages.UnauthorizedAction);

            var pendingOrder = await orderRepository.GetPendingOrderById(Guid.Parse(userId), request.Id, cancellationToken);
            if (pendingOrder is null)
                return Result.Failure(ErrorMessages.NoPendingOrders);

            pendingOrder.UpdateStatus(OrderStatus.Cancelled);
            await unitOfWork.Commit();
            await cache.RemoveAsync($"{CacheKeys.UserOrders}_{userId}", cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.UnexpectedError);
            return Result.Failure(ex.Message);
        }
    }
}