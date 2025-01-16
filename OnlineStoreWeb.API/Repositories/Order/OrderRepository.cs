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
        catch (Exception ex)
        {
            throw new Exception("Error fetching orders", ex);
        }
    }

    public async Task<Order?> GetOrderWithIdAsync(int id)
    {
        try
        {
            return await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
        }
        catch (Exception ex)
        {
            throw new Exception("Error fetching order", ex);
        }
    }

    public async Task AddOrderAsync(CreateOrderDto createOrderDto)
    {
        try
        {
            var order = new Order 
            {
                Product = createOrderDto.Product,
                OrderCreated = createOrderDto.OrderCreated
            };
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Error adding order", ex);
        }
    }

    public async Task UpdateOrderAsync(UpdateOrderDto updateOrderDto)
    {
        try
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == updateOrderDto.Id) 
                ?? throw new Exception("Order not found");

            order.Product = updateOrderDto.Product;
            order.OrderUpdated = updateOrderDto.OrderUpdated;
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Error updating order", ex);
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
        catch (Exception ex)
        {
            throw new Exception("Error deleting order", ex);
        }
    }
}