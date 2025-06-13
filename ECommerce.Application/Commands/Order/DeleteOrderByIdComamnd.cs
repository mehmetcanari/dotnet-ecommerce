using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;

namespace ECommerce.Application.Commands.Order;

public class DeleteOrderByIdCommand : IRequest<Result>
{
    public required int Id { get; set; }
}

public class DeleteOrderByIdCommandHandler : IRequestHandler<DeleteOrderByIdCommand, Result>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILoggingService _logger;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteOrderByIdCommandHandler(
        IOrderRepository orderRepository,
        ILoggingService logger,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteOrderByIdCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var orderResult = await ValidateAndGetOrder(request);
            if (orderResult.IsFailure)
                return Result.Failure(orderResult.Error);

            await DeleteOrder(orderResult.Data);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting order: {Message}", ex.Message);
            return Result.Failure(ex.Message);
        }
    }

    private async Task<Result<Domain.Model.Order>> ValidateAndGetOrder(DeleteOrderByIdCommand request)
    {
        var order = await _orderRepository.GetOrderById(request.Id);
        if (order == null)
        {
            _logger.LogWarning("Order not found with ID: {OrderId}", request.Id);
            return Result<Domain.Model.Order>.Failure("Order not found");
        }

        return Result<Domain.Model.Order>.Success(order);
    }

    private async Task DeleteOrder(Domain.Model.Order order)
    {
        _orderRepository.Delete(order);
        await _unitOfWork.Commit();
        
        _logger.LogInformation("Order deleted successfully: {OrderId}, {OrderStatus}", 
            order.OrderId, order.Status);
    }
}