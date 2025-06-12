using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Order;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;

public class UpdateOrderStatusByAccountIdCommand : IRequest<Result>
{
    public int AccountId { get; set; }
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
            var order = await _orderRepository.GetOrderByAccountId(request.AccountId);
            if (order == null)
            {
                _logger.LogWarning("Order not found for account id: {AccountId}", request.AccountId);
                return Result.Failure("Order not found");
            }
            
            order.Status = request.OrderUpdateRequestDto.Status;
            _orderRepository.Update(order);

            _logger.LogInformation("Order status updated successfully: {Order}", order);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating order status: {Message}", ex.Message);
            return Result.Failure(ex.Message);
        }
    }
}