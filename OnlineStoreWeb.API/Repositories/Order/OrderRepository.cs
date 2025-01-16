using Microsoft.EntityFrameworkCore;

public class OrderRepository : IOrderRepository
{
    private readonly StoreDbContext _context;

    public OrderRepository(StoreDbContext context)
    {
        _context = context;
    }

    public async Task<List<Order>> GetAllOrdersAsync()
    {
        try
        {
            return await _context.Orders.ToListAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("Failed to fetch orders", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task<Order?> GetOrderWithIdAsync(int id)
    {
        try
        {
            return await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("Failed to fetch order", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task AddOrderAsync(CreateOrderDto createOrderDto)
    {
        try
        {
            var order = new Order 
            {
                OrderItem = createOrderDto.OrderItem,
                OrderCreated = createOrderDto.OrderCreated
            };
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("Failed to save order", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task UpdateOrderAsync(UpdateOrderDto updateOrderDto)
    {
        try
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == updateOrderDto.Id) 
                ?? throw new Exception("Order not found");

            order.OrderItem = updateOrderDto.OrderItem;
            order.OrderUpdated = updateOrderDto.OrderUpdated;
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("Failed to update order", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task DeleteOrderAsync(int id)
    {
        try
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id)
                ?? throw new Exception("Order not found");

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("Failed to delete order", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }
}