using Microsoft.EntityFrameworkCore;

public class OrderItemRepository :  IOrderItemRepository
{
    private readonly StoreDbContext _context;

    public OrderItemRepository(StoreDbContext context)
    {
        _context = context;
    }

    public async Task<List<OrderItem>> Get()
    {
        try
        {
            return await _context.OrderItems.AsNoTracking().ToListAsync(); 
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

    public async Task Add(OrderItem orderItem)
    {
        try
        {
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

    public async Task Update(OrderItem orderItem)
    {
        try
        {
            _context.OrderItems.Update(orderItem);
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

    public async Task Delete(OrderItem orderItem)
    {
        try
        {
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