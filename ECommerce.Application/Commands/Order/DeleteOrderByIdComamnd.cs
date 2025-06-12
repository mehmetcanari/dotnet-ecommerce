using ECommerce.Application.Abstract.Service;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;

public class DeleteOrderByIdCommand : IRequest<Result>
{
    public int Id { get; set; }
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
            var activeOrder = await _orderRepository.GetOrderById(request.Id);
            if (activeOrder == null)
            {
                _logger.LogWarning("Order not found: {Id}", request.Id);
                return Result.Failure("Order not found");
            }
            
            _orderRepository.Delete(activeOrder);

            await _unitOfWork.Commit();
            _logger.LogInformation("Order deleted successfully: {Order}", activeOrder);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting order: {Message}", ex.Message);
            return Result.Failure(ex.Message);
        }
    }
}