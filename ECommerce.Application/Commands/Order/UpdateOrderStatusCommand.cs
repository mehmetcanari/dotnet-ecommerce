using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Order;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Commands.Order;

public class UpdateOrderStatusCommand : IRequest<Result>
{
    public required string UserId { get; set; }
    public required UpdateOrderStatusRequestDto Request { get; set; }
}

public class UpdateOrderStatusByAccountIdCommandHandler : IRequestHandler<UpdateOrderStatusCommand, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILoggingService _logger;

    public UpdateOrderStatusByAccountIdCommandHandler(IOrderRepository orderRepository, ILoggingService logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var orderResult = await ValidateAndGetOrder(request.UserId);
            if (!orderResult.IsSuccess && orderResult.Message is not null)
                return Result.Failure(orderResult.Message);
            
            if (orderResult.Data is null)
                return Result.Failure(ErrorMessages.OrderNotFound);
            
            UpdateOrderStatus(orderResult.Data, request.Request.Status);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.UnexpectedError, request.UserId, ex.Message);
            return Result.Failure(ErrorMessages.UnexpectedError);
        }
    }

    private async Task<Result<Domain.Model.Order>> ValidateAndGetOrder(string userId)
    {
        var order = await _orderRepository.GetOrderByAccountId(userId);
        if (order == null)
        {
            _logger.LogWarning(ErrorMessages.OrderNotFound, userId);
            return Result<Domain.Model.Order>.Failure(ErrorMessages.OrderNotFound);
        }

        return Result<Domain.Model.Order>.Success(order);
    }

    private void UpdateOrderStatus(Domain.Model.Order order, OrderStatus newStatus)
    {
        order.Status = newStatus;
        _orderRepository.Update(order);
    }
}