using Microsoft.EntityFrameworkCore;
using OnlineStoreWeb.API.Model;

namespace OnlineStoreWeb.API.Repositories.OrderItem;

public class OrderItemRepository(StoreDbContext context) : IOrderItemRepository
{
    public async Task<List<Model.OrderItem>> Get()
    {
        try
        {
            return await context.OrderItems.AsNoTracking().ToListAsync(); 
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

    public async Task Add(Model.OrderItem orderItem)
    {
        try
        {
            await context.OrderItems.AddAsync(orderItem);
            await context.SaveChangesAsync();
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

    public async Task Update(Model.OrderItem orderItem)
    {
        try
        {
            context.OrderItems.Update(orderItem);
            await context.SaveChangesAsync();
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

    public async Task Delete(Model.OrderItem orderItem)
    {
        try
        {
            context.OrderItems.Remove(orderItem);
            await context.SaveChangesAsync();
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