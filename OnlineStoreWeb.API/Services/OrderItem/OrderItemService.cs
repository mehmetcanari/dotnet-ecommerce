public class OrderItemService : IOrderItemService
{
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<OrderItemService> _logger;

    public OrderItemService(IOrderItemRepository orderItemRepository, IProductRepository productRepository, ILogger<OrderItemService> logger)
    {
        _orderItemRepository = orderItemRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<List<OrderItem>> GetAllOrderItemsAsync()
    {
        try
        {
            List<OrderItem> orderItems = await _orderItemRepository.Get();
            return orderItems;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching all order items");
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task<List<OrderItem>> GetAllOrderItemsWithUserIdAsync(int userId)
    {
        try
        {
            List<OrderItem> orderItems = await _orderItemRepository.Get();
            List<OrderItem> userOrderItems = orderItems.Where(o => o.UserId == userId).ToList();
            return userOrderItems;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching all order items with user id: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task<OrderItem> GetSpecifiedOrderItemsWithUserIdAsync(int userId, int orderItemId)
    {
        try
        {
            List<OrderItem> orderItems = await _orderItemRepository.Get();
            List<OrderItem> userOrderItems = orderItems.Where(o => o.UserId == userId).ToList();

            OrderItem orderItem = userOrderItems.FirstOrDefault(o => o.Id == orderItemId)
                ?? throw new Exception("OrderItem not found");
            return orderItem;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while fetching specified order items with user id: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task AddOrderItemAsync(CreateOrderItemDto createOrderItemRequest)
    {
        try
        {
            List<OrderItem> orderItems = await _orderItemRepository.Get();
            List<Product> products = await _productRepository.Get();

            Product fetchedProduct = products.FirstOrDefault(p => p.Id == createOrderItemRequest.ProductId)
                ?? throw new Exception("Product not found");

            if(orderItems.Any(o => o.UserId == createOrderItemRequest.UserId && o.ProductId == createOrderItemRequest.ProductId)) //Duplicate order item check
            {
                throw new Exception("OrderItem already exists");
            }

            OrderItem orderItem = new OrderItem
            {
                Quantity = createOrderItemRequest.Quantity,
                UserId = createOrderItemRequest.UserId,
                ProductId = createOrderItemRequest.ProductId,
                Price = fetchedProduct.Price,
                OrderItemUpdated = DateTime.UtcNow
            };

            await _orderItemRepository.Add(orderItem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while adding order item: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task UpdateOrderItemAsync(UpdateOrderItemDto updateOrderItemRequest)
    {
        try
        {
            List<OrderItem> orderItems = await _orderItemRepository.Get();
            List<Product> products = await _productRepository.Get();

            Product fetchedProduct = products.FirstOrDefault(p => p.Id == updateOrderItemRequest.ProductId)
                ?? throw new Exception("Product not found");

            OrderItem orderItem = orderItems.FirstOrDefault(
                o => o.Id == updateOrderItemRequest.Id 
            && o.UserId == updateOrderItemRequest.UserId)

                ?? throw new Exception("OrderItem not found");

            orderItem.Quantity = updateOrderItemRequest.Quantity;
            orderItem.ProductId = updateOrderItemRequest.ProductId;
            orderItem.Price = fetchedProduct.Price;
            orderItem.OrderItemUpdated = DateTime.UtcNow;

            await _orderItemRepository.Update(orderItem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating order item: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task DeleteSpecifiedUserOrderItemAsync(int userId, int orderItemId)
    {
        try
        {
            List<OrderItem> orderItems = await _orderItemRepository.Get();
            OrderItem orderItem = orderItems.FirstOrDefault(o => o.UserId == userId && o.Id == orderItemId)
                ?? throw new Exception("OrderItem not found");

            await _orderItemRepository.Delete(orderItem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting specified order item with user id: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task DeleteAllUserOrderItemsAsync(int userId)
    {
        try
        {
            List<OrderItem> orderItems = await _orderItemRepository.Get();
            List<OrderItem> userOrderItems = orderItems.Where(o => o.UserId == userId).ToList();
            foreach(OrderItem orderItem in userOrderItems)
            {
                await _orderItemRepository.Delete(orderItem);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting all order items with user id: {Message}", ex.Message);
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task DeleteAllOrderItemsAsync()
    {
        try
        {
            List<OrderItem> orderItems = await _orderItemRepository.Get();
            foreach(OrderItem orderItem in orderItems)
            {
                await _orderItemRepository.Delete(orderItem);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting all order items");
            throw new Exception("An unexpected error occurred", ex);
        }
    }
}