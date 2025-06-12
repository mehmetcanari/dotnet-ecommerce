using ECommerce.Application.Abstract.Service;
using ECommerce.Application.DTO.Response.BasketItem;
using ECommerce.Application.DTO.Response.Order;
using ECommerce.Application.Utility;
using ECommerce.Domain.Abstract.Repository;
using MediatR;

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
            var orders = await _orderRepository.Read();
            if (orders.Count == 0)
            {
                return Result<List<OrderResponseDto>>.Failure("No orders found");
            }

            var items = orders.Select(o => new OrderResponseDto
            {
                AccountId = o.AccountId,
                BasketItems = o.BasketItems.Select(oi => new BasketItemResponseDto
                {
                    AccountId = oi.AccountId,
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    ProductName = oi.ProductName
                }).ToList(),
                OrderDate = o.OrderDate,
                ShippingAddress = o.ShippingAddress,
                BillingAddress = o.BillingAddress,
                Status = o.Status
            }).ToList();

            return Result<List<OrderResponseDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching all orders: {Message}", ex.Message);
            return Result<List<OrderResponseDto>>.Failure("An unexpected error occurred");
        }
    }
}