using OnlineStoreWeb.API.DTO.Request.Order;
using OnlineStoreWeb.API.DTO.Response.Order;
using OnlineStoreWeb.API.DTO.Response.OrderItem;
using OnlineStoreWeb.API.Repositories.Account;
using OnlineStoreWeb.API.Repositories.Order;
using OnlineStoreWeb.API.Repositories.OrderItem;
using OnlineStoreWeb.API.Repositories.Product;

namespace OnlineStoreWeb.API.Services.Order;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IOrderRepository orderRepository, IOrderItemRepository orderItemRepository,
        IAccountRepository accountRepository, ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _accountRepository = accountRepository;
        _orderItemRepository = orderItemRepository;
        _logger = logger;
    }

    public async Task AddOrderAsync(OrderCreateDto createOrderDto)
    {
        try
        {
            var orderItems = await _orderItemRepository.Read();
            var accounts = await _accountRepository.Read();

            Model.Account userAccount = accounts.FirstOrDefault(a => a.AccountId == createOrderDto.AccountId) ??
                                        throw new Exception("User not found");

            var userOrderItems = orderItems
                .Where(oi => oi.AccountId == userAccount.AccountId)
                .Select(item => new Model.OrderItem
                {
                    AccountId = item.AccountId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price
                }).ToList();

            if (userOrderItems.Count == 0)
            {
                throw new Exception("No items in cart");
            }

            Model.Order order = new Model.Order
            {
                AccountId = userAccount.AccountId,
                ShippingAddress = createOrderDto.ShippingAddress,
                BillingAddress = createOrderDto.BillingAddress,
                PaymentMethod = createOrderDto.PaymentMethod,
                OrderItems = userOrderItems
            };

            await _orderRepository.Create(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while adding order: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task DeleteOrderAsync(int userId)
    {
        try
        {
            List<Model.Order> orders = await _orderRepository.Read();
            Model.Order order = orders.FirstOrDefault(o => o.OrderId == userId) ??
                                throw new Exception("Order not found");
            await _orderRepository.Delete(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting order: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task<List<OrderResponseDto>> GetAllOrdersAsync()
    {
        try
        {
            List<Model.Order> orders = await _orderRepository.Read();
            
            return orders.Select(o => new OrderResponseDto
            {
                AccountId = o.AccountId,
                OrderItems = o.OrderItems.Select(oi => new OrderItemResponseDto
                {
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    Price = oi.Price
                }).ToList(),
                TotalPrice = o.OrderItems.Sum(oi => oi.Price),
                OrderDate = o.OrderDate,
                ShippingAddress = o.ShippingAddress,
                BillingAddress = o.BillingAddress,
                PaymentMethod = o.PaymentMethod,
                Status = o.Status
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching all orders: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task<OrderResponseDto> GetOrderWithIdAsync(int id)
    {
        try
        {
            var order = await _orderRepository.GetOrderById(id);
            if (order == null)
                throw new Exception("Order not found");
        
            OrderResponseDto orderResponseDto = new()
            {
                AccountId = order.AccountId,
                OrderItems = order.OrderItems.Select(oi => new OrderItemResponseDto
                {
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    Price = oi.Price
                }).ToList(),
                TotalPrice = order.OrderItems.Sum(oi => oi.Price),
                OrderDate = order.OrderDate,
                ShippingAddress = order.ShippingAddress,
                BillingAddress = order.BillingAddress,
                PaymentMethod = order.PaymentMethod,
                Status = order.Status
            };
            
            return orderResponseDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching order with id: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task UpdateOrderStatusAsync(int id, OrderUpdateDto orderUpdateDto)
    {
        try
        {
            List<Model.Order> orders = await _orderRepository.Read();
            Model.Order order = orders.FirstOrDefault(o => o.OrderId == id) ?? throw new Exception("Order not found");
            order.Status = orderUpdateDto.Status;
            await _orderRepository.Update(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating order status: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }
}