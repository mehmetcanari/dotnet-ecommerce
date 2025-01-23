using Microsoft.EntityFrameworkCore;
using OnlineStoreWeb.API.Model;

namespace OnlineStoreWeb.API.Repositories.Order;

public class OrderRepository(StoreDbContext context) : IOrderRepository
{
    public async Task<List<Model.Order>> Get()
    {
        try
        {
            return await context.Orders.ToListAsync();
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

    public async Task Add(Model.Order order)
    {
        try
        {
            await context.Orders.AddAsync(order);
            await context.SaveChangesAsync();
        }
        catch (Exception exception)
        {
            throw new Exception(exception.Message);
        }
    }

    public async Task Update(Model.Order order)
    {
        try
        {
            context.Orders.Update(order);
            await context.SaveChangesAsync();
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

    public async Task Delete(Model.Order order)
    {
        try
        {
            context.Orders.Remove(order);
            await context.SaveChangesAsync();
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