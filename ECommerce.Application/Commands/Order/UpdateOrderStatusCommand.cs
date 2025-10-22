using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Request.Order;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Shared.Constants;
using MediatR;

namespace ECommerce.Application.Commands.Order;

public class UpdateOrderStatusCommand : IRequest<Result>
{
    public required int AccountId { get; set; }
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
            var orderResult = await ValidateAndGetOrder(request.AccountId);
            if (!orderResult.IsSuccess && orderResult.Error is not null)
                return Result.Failure(orderResult.Error);
            
            if (orderResult.Data is null)
                return Result.Failure(ErrorMessages.OrderNotFound);
            
            UpdateOrderStatus(orderResult.Data, request.Request.Status);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ErrorMessages.UnexpectedError, request.AccountId, ex.Message);
            return Result.Failure(ErrorMessages.UnexpectedError);
        }
    }

    private async Task<Result<Domain.Model.Order>> ValidateAndGetOrder(int accountId)
    {
        var order = await _orderRepository.GetOrderByAccountId(accountId);
        if (order == null)
        {
            _logger.LogWarning(ErrorMessages.OrderNotFound, accountId);
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