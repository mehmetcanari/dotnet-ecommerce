using ECommerce.Application.Abstract;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using ECommerce.Shared.Wrappers;
using MediatR;

namespace ECommerce.Application.Commands.Order;

public class DeleteOrderByIdCommand(Guid id) : IRequest<Result>
{
    public readonly Guid Id = id;
}

public class DeleteOrderByIdCommandHandler(IOrderRepository orderRepository, ILogService logger, IUnitOfWork unitOfWork, ICacheService cache, ILockProvider lockProvider) : IRequestHandler<DeleteOrderByIdCommand, Result>
{
    public async Task<Result> Handle(DeleteOrderByIdCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await orderRepository.GetById(request.Id, cancellationToken);
            if (order is null)
                return Result.Failure(ErrorMessages.OrderNotFound);

            using (await lockProvider.AcquireLockAsync($"order:{order.Id}", cancellationToken))
            {
                await cache.RemoveAsync($"{CacheKeys.UserOrders}_{order.UserId}", cancellationToken);
                orderRepository.Delete(order);
                await unitOfWork.Commit();
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.UnexpectedError, ex.Message);
            return Result.Failure(ex.Message);
        }
    }
}