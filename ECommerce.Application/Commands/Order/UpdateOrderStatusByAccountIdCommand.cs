using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Order;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;

namespace ECommerce.Application.Commands.Order;

public class UpdateOrderStatusByAccountIdCommand : IRequest<Result>
{
    public required int AccountId { get; set; }
    public required OrderUpdateRequestDto OrderUpdateRequestDto { get; set; }
}

public class UpdateOrderStatusByAccountIdCommandHandler : IRequestHandler<UpdateOrderStatusByAccountIdCommand, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILoggingService _logger;

    public UpdateOrderStatusByAccountIdCommandHandler(
        IOrderRepository orderRepository,
        ILoggingService logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateOrderStatusByAccountIdCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var orderResult = await ValidateAndGetOrder(request.AccountId);
            if (!orderResult.IsSuccess)
            {
                return Result.Failure(orderResult.Error);
            }

            UpdateOrderStatus(orderResult.Data, request.OrderUpdateRequestDto.Status);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating order status for account {AccountId}: {Message}", 
                request.AccountId, ex.Message);
            return Result.Failure("An unexpected error occurred while updating the order status");
        }
    }

    private async Task<Result<Domain.Model.Order>> ValidateAndGetOrder(int accountId)
    {
        var order = await _orderRepository.GetOrderByAccountId(accountId);
        if (order == null)
        {
            _logger.LogWarning("Order not found for account id: {AccountId}", accountId);
            return Result<Domain.Model.Order>.Failure("Order not found");
        }

        return Result<Domain.Model.Order>.Success(order);
    }

    private void UpdateOrderStatus(Domain.Model.Order order, OrderStatus newStatus)
    {
        order.Status = newStatus;
        _orderRepository.Update(order);

        _logger.LogInformation("Order status updated successfully. OrderId: {OrderId}, NewStatus: {Status}", 
            order.OrderId, newStatus);
    }
}