using Microsoft.EntityFrameworkCore;

public class OrderRepository : IOrderRepository
{
    private readonly StoreDbContext _context;
    private readonly IOrderItemRepository _orderItemRepository;

    public OrderRepository(StoreDbContext context, IOrderItemRepository orderItemRepository)
    {
        _context = context;
        _orderItemRepository = orderItemRepository;
    }

    public async Task<List<Order>> Get()
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

    public async Task Add(Order order)
    {
        try
        {
            
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

    public async Task Update(Order order)
    {
        try
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new DbUpdateException("Failed to update order status", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task Delete(Order order)
    {
        try
        {
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