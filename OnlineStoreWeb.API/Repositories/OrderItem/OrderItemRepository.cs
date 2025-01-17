using Microsoft.EntityFrameworkCore;

public class OrderItemRepository : IOrderItemRepository
{
    private readonly StoreDbContext _context;

    public OrderItemRepository(StoreDbContext context)
    {
        _context = context;
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

    public async Task<OrderItem?> GetOrderItemWithIdAsync(int id)
    {
        try
        {
            return await _context.OrderItems.FirstOrDefaultAsync(o => o.Id == id);
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

    public async Task AddOrderItemAsync(CreateOrderItemDto createOrderItemDto)
    {
        try
        {
        var orderItem = new OrderItem
        {
            Quantity = createOrderItemDto.Quantity,
            Product = createOrderItemDto.Product,
            OrderItemCreated = createOrderItemDto.OrderItemCreated,
            OrderItemUpdated = createOrderItemDto.OrderItemUpdated
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

    public async Task UpdateOrderItemAsync(int id, UpdateOrderItemDto updateOrderItemDto)
    {
        try
        {
            var orderItem = await _context.OrderItems.FirstOrDefaultAsync(o => o.Id == id)
                ?? throw new Exception("OrderItem not found");

            orderItem.Quantity = updateOrderItemDto.Quantity;
            orderItem.Product = updateOrderItemDto.Product;
            orderItem.OrderItemUpdated = updateOrderItemDto.OrderItemUpdated;
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

    public async Task DeleteOrderItemAsync(int id)
    {
        try
        {
            var orderItem = await _context.OrderItems.FirstOrDefaultAsync(o => o.Id == id)
                ?? throw new Exception("OrderItem not found");

            _context.OrderItems.Remove(orderItem);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("Failed to delete order item", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }
}