using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Response.BasketItem;
using ECommerce.Application.DTO.Response.Order;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;

namespace ECommerce.Application.Queries.Order;

public class GetAllOrdersQuery : IRequest<Result<List<OrderResponseDto>>>{}

public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, Result<List<OrderResponseDto>>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILoggingService _logger;

    public GetAllOrdersQueryHandler(IOrderRepository orderRepository, ILoggingService logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<Result<List<OrderResponseDto>>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var orders = await GetOrders();
            if (orders.Count == 0)
            {
                return Result<List<OrderResponseDto>>.Failure("No orders found");
            }

            var orderDtos = orders.Select(MapToResponseDto).ToList();
            return Result<List<OrderResponseDto>>.Success(orderDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching all orders: {Message}", ex.Message);
            return Result<List<OrderResponseDto>>.Failure("An unexpected error occurred");
        }
    }

    private async Task<List<Domain.Model.Order>> GetOrders()
    {
        return await _orderRepository.Read();
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