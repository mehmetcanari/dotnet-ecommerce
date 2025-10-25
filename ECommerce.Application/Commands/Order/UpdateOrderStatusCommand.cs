using ECommerce.Application.Abstract.Service;
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

public class UpdateOrderStatusByAccountIdCommandHandler(IOrderRepository orderRepository, ILoggingService logger) : IRequestHandler<UpdateOrderStatusCommand, Result>
{
    public async Task<Result> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var orderResult = await ValidateAndGetOrder(request.Model.UserId);
            if (orderResult is { IsSuccess: false, Message: not null })
                return Result.Failure(orderResult.Message);
            
            if (orderResult.Data is null)
                return Result.Failure(ErrorMessages.OrderNotFound);
            
            UpdateOrderStatus(orderResult.Data, request.Model.Status);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.UnexpectedError, request.Model.UserId, ex.Message);
            return Result.Failure(ErrorMessages.UnexpectedError);
        }
    }

    private async Task<Result<Domain.Model.Order>> ValidateAndGetOrder(Guid userId)
    {
        var order = await orderRepository.GetByUserId(userId);
        if (order == null)
            return Result<Domain.Model.Order>.Failure(ErrorMessages.OrderNotFound);

        return Result<Domain.Model.Order>.Success(order);
    }

    private void UpdateOrderStatus(Domain.Model.Order order, OrderStatus newStatus)
    {
        order.Status = newStatus;
        orderRepository.Update(order);
    }
}