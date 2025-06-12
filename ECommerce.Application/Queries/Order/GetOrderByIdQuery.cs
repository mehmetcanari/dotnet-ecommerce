using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Response.BasketItem;
using ECommerce.Application.DTO.Response.Order;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;

public class GetOrderByIdQuery : IRequest<Result<OrderResponseDto>>
{
    public int OrderId { get; set; }
}

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderResponseDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILoggingService _logger;

    public GetOrderByIdQueryHandler(IOrderRepository orderRepository, ILoggingService logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<Result<OrderResponseDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderRepository.GetOrderById(request.OrderId);
            if (order == null)
            {
                _logger.LogWarning("Order not found: {Id}", request.OrderId);
                return Result<OrderResponseDto>.Failure("Order not found");
            }

            OrderResponseDto orderResponseDto = new()
            {
                AccountId = order.AccountId,
                BasketItems = order.BasketItems.Select(oi => new BasketItemResponseDto
                {
                    AccountId = oi.AccountId,
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    ProductName = oi.ProductName
                }).ToList(),
                OrderDate = order.OrderDate,
                ShippingAddress = order.ShippingAddress,
                BillingAddress = order.BillingAddress,
                Status = order.Status
            };

            return Result<OrderResponseDto>.Success(orderResponseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching order with id: {Message}", ex.Message);
            return Result<OrderResponseDto>.Failure("An unexpected error occurred");
        }
    }
}