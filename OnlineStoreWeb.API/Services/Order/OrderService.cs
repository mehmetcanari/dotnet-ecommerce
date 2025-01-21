
public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly ILogger<OrderService> _logger;


    public OrderService(IOrderRepository orderRepository, IOrderItemRepository orderItemRepository, ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
        _logger = logger;
    }

    public async Task AddOrderAsync(OrderCreateDto createOrderDto)
    {
        try
        {
            List<OrderItem> orderItems = await _orderItemRepository.Get();
            List<OrderItem> orderItemsByUserId = orderItems.Where(o => o.UserId == createOrderDto.UserId).ToList();

            if(orderItemsByUserId.Count == 0)
            {
                _logger.LogError("No order items found for the user");
                throw new Exception("No order items found for the user");
            }

            Order order = new Order
            {
                UserId = createOrderDto.UserId,
                ShippingAddress = createOrderDto.ShippingAddress,
                PaymentMethod = createOrderDto.PaymentMethod,
                Status = OrderStatus.Pending,
                OrderItems = orderItemsByUserId
            };
            await _orderRepository.Add(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while adding order: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task DeleteOrderAsync(int id)
    {
        try
        {
            List<Order> orders = await _orderRepository.Get();
            Order order = orders.FirstOrDefault(o => o.Id == id) ?? throw new Exception("Order not found");
            await _orderRepository.Delete(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting order: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task DeleteOrderWithUserIdAsync(int userId)
    {
        try
        {
            List<Order> orders = await _orderRepository.Get();
            Order order = orders.FirstOrDefault(o => o.UserId == userId) ?? throw new Exception("Order not found");
            await _orderRepository.Delete(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting order with user id: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task<List<Order>> GetAllOrdersAsync()
    {
        try
        {
            List<Order> orders = await _orderRepository.Get();
            return orders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching all orders: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task<List<Order>> GetOrdersByUserIdAsync(int userId)
    {
        try
        {
            List<Order> orders = await _orderRepository.Get();
            List<Order> ordersByUserId = orders.Where(o => o.UserId == userId).ToList();
            return ordersByUserId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching orders by user id: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task<Order> GetOrderWithIdAsync(int id)
    {
        try
        {
            List<Order> orders = await _orderRepository.Get();
            Order order = orders.FirstOrDefault(o => o.Id == id) ?? throw new Exception("Order not found");
            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching order with id: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task UpdateOrderStatusAsync(int id, OrderStatus status)
    {
        try
        {
            List<Order> orders = await _orderRepository.Get();
            Order order = orders.FirstOrDefault(o => o.Id == id) ?? throw new Exception("Order not found");
            order.Status = status;
            await _orderRepository.Update(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating order status: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }
}