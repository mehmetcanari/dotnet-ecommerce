using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Response.BasketItem;
using ECommerce.Application.DTO.Response.Order;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;

namespace ECommerce.Application.Queries.Order;

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

            var orderResponseDto = MapToResponseDto(order);
            return Result<OrderResponseDto>.Success(orderResponseDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching order with id: {Message}", ex.Message);
            return Result<OrderResponseDto>.Failure("An unexpected error occurred");
        }
    }

    private static OrderResponseDto MapToResponseDto(Domain.Model.Order order)
    {
        return new OrderResponseDto
        {
            AccountId = order.AccountId,
            BasketItems = order.BasketItems.Select(MapToBasketItemDto).ToList(),
            OrderDate = order.OrderDate,
            ShippingAddress = order.ShippingAddress,
            BillingAddress = order.BillingAddress,
            Status = order.Status
        };
    }

    private static BasketItemResponseDto MapToBasketItemDto(Domain.Model.BasketItem basketItem)
    {
        return new BasketItemResponseDto
        {
            AccountId = basketItem.AccountId,
            ProductId = basketItem.ProductId,
            Quantity = basketItem.Quantity,
            UnitPrice = basketItem.UnitPrice,
            ProductName = basketItem.ProductName
        };
    }
}