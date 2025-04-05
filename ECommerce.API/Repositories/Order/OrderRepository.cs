using ECommerce.API.Model;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Repositories.Order;

public class OrderRepository(StoreDbContext context) : IOrderRepository
{
    public async Task<List<Model.Order>> Read()
    {
        try
        {
            return await context.Orders
                .Include(o => o.OrderItems)
                .ToListAsync();
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

    public async Task Create(Model.Order order)
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

    #region IQueryable

    public async Task<Model.Order> GetOrderById(int id)
    {
        try
        {
            return await context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == id) ?? throw new Exception("Order not found");
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    #endregion
}