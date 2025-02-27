using OnlineStoreWeb.API.DTO.Request.Order;
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

            var userOrderItems = orderItems.Where(oi => oi.AccountId == userAccount.AccountId).ToList();

            if (userOrderItems.Count == 0)
            {
                throw new Exception("No items in cart");
            }

            List<Model.OrderItem> newOrderItems = userOrderItems
                .Select(oi => new Model.OrderItem
                {
                    AccountId = oi.AccountId,
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    Price = oi.Price,
                })
                .ToList();

            Model.Order order = new Model.Order
            {
                AccountId = userAccount.AccountId,
                ShippingAddress = createOrderDto.ShippingAddress,
                BillingAddress = createOrderDto.BillingAddress,
                PaymentMethod = createOrderDto.PaymentMethod,
                OrderItems = newOrderItems
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

    public async Task<List<Model.Order>> GetAllOrdersAsync()
    {
        try
        {
            List<Model.Order> orders = await _orderRepository.Read();
            return orders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching all orders: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task<Model.Order> GetOrderWithIdAsync(int id)
    {
        try
        {
            var order = await _orderRepository.GetOrderById(id);
            if (order == null)
                throw new Exception("Order not found");
        
            return order;
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