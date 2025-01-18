using Microsoft.EntityFrameworkCore;

public class OrderItemRepository :  IOrderItemRepository
{
    private readonly StoreDbContext _context;
    private readonly IProductRepository _productRepository;

    public OrderItemRepository(StoreDbContext context, IProductRepository productRepository)
    {
        _context = context;
        _productRepository = productRepository;
    }

    public async Task<List<OrderItem>> GetAllOrderItemsAsync()
    {
        try
        {
            return await _context.OrderItems.ToListAsync(); 
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("Failed to fetch order items", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task<List<OrderItem>> GetAllOrderItemsWithUserIdAsync(int userId)
    {
        try
        {
            List<OrderItem> orderItems = await _context.OrderItems.Where(o => o.UserId == userId).ToListAsync();
            return orderItems;
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("Failed to fetch order items", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task<OrderItem?> GetSpecifiedOrderItemsWithUserIdAsync(int userId, int orderItemId)
    {
        try
        {
            OrderItem orderItem = await _context.OrderItems.FirstOrDefaultAsync(o => o.UserId == userId && o.Id == orderItemId)
                ?? throw new Exception("OrderItem not found");
            return orderItem;
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("Failed to fetch order item", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task AddOrderItemAsync(CreateOrderItemDto createOrderItemRequest)
    {
        try
        {
            Product fetchedProduct = await _productRepository.GetProductWithIdAsync(createOrderItemRequest.ProductId)
                ?? throw new Exception("Product not found");

            OrderItem orderItem = new OrderItem
            {
                Quantity = createOrderItemRequest.Quantity,
                UserId = createOrderItemRequest.UserId,
                ProductId = createOrderItemRequest.ProductId,
                Price = fetchedProduct.Price,
                OrderItemUpdated = DateTime.UtcNow
            };

            await _context.OrderItems.AddAsync(orderItem);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("Failed to save order item", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task UpdateOrderItemAsync(int id, UpdateOrderItemDto updateOrderItemRequest)
    {
        try
        {
            OrderItem orderItem = await _context.OrderItems.FirstOrDefaultAsync(o => o.Id == id && o.UserId == updateOrderItemRequest.UserId)
                ?? throw new Exception("OrderItem not found");

            Product fetchedProduct = await _productRepository.GetProductWithIdAsync(updateOrderItemRequest.ProductId)
                ?? throw new Exception("Product not found");

            orderItem.Quantity = updateOrderItemRequest.Quantity;
            orderItem.ProductId = updateOrderItemRequest.ProductId;
            orderItem.Price = fetchedProduct.Price;
            orderItem.OrderItemUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("Failed to update order item", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task DeleteSpecifiedUserOrderItemAsync(int userId, int orderItemId)
    {
        try
        {
            OrderItem orderItem = await _context.OrderItems.FirstOrDefaultAsync(o => o.UserId == userId && o.Id == orderItemId)
                ?? throw new Exception("OrderItem not found");

            _context.OrderItems.Remove(orderItem);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("Failed to delete order items", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task DeleteAllUserOrderItemsAsync(int userId)
    {
        try
        {
            List<OrderItem> orderItems = await _context.OrderItems.Where(o => o.UserId == userId).ToListAsync();
            _context.OrderItems.RemoveRange(orderItems);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("Failed to delete order items", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task DeleteAllOrderItemsAsync()
    {
        try
        {
            List<OrderItem> orderItems = await _context.OrderItems.ToListAsync();
            _context.OrderItems.RemoveRange(orderItems);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("Failed to delete order items", ex);
        }
    }
}