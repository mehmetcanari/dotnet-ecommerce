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
        catch (Exception ex)
        {
            throw new Exception("Error fetching order items", ex);
        }
    }

    public async Task<OrderItem?> GetOrderItemWithIdAsync(int id)
    {
        try
        {
            return await _context.OrderItems.FirstOrDefaultAsync(o => o.Id == id);
        }
        catch (Exception ex)
        {
            throw new Exception("Error fetching order item", ex);
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
            OrderItemCreated = createOrderItemDto.OrderItemCreated
        };

            await _context.OrderItems.AddAsync(orderItem);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Error adding order item", ex);
        }
    }

    public async Task UpdateOrderItemAsync(UpdateOrderItemDto updateOrderItemDto)
    {
        try
        {
            var orderItem = await _context.OrderItems.FirstOrDefaultAsync(o => o.Id == updateOrderItemDto.Id)
                ?? throw new Exception("OrderItem not found");

        orderItem.Quantity = updateOrderItemDto.Quantity;
        orderItem.Product = updateOrderItemDto.Product;
            orderItem.OrderItemUpdated = updateOrderItemDto.OrderItemUpdated;
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Error updating order item", ex);
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
        catch (Exception ex)
        {
            throw new Exception("Error deleting order item", ex);
        }
    }
}