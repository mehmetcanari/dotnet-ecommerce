using ECommerce.Application.Abstract;
using ECommerce.Application.DTO.Request.Order;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Commands.Order;

public class UpdateOrderStatusCommand(UpdateOrderStatusRequestDto request) : IRequest<Result>
{
    public readonly UpdateOrderStatusRequestDto Model = request;
}

public class UpdateOrderStatusCommandHandler(IOrderRepository orderRepository, ILogService logger, IUnitOfWork unitOfWork, ICacheService cache) : IRequestHandler<UpdateOrderStatusCommand, Result>
{
    public async Task<Result> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await orderRepository.GetByUserId(request.Model.UserId, request.Model.OrderId, cancellationToken);
            if (order is null)
                return Result.Failure(ErrorMessages.OrderNotFound);

            order.UpdateStatus(request.Model.Status);
            orderRepository.Update(order);
            await cache.RemoveAsync($"{CacheKeys.UserOrders}_{order.UserId}", cancellationToken);
            await unitOfWork.Commit();

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.UnexpectedError, request.Model.UserId, ex.Message);
            return Result.Failure(ErrorMessages.UnexpectedError);
        }
    }
}