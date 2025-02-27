using Microsoft.EntityFrameworkCore;
using OnlineStoreWeb.API.Model;

namespace OnlineStoreWeb.API.Repositories.OrderItem;

public class OrderItemRepository(StoreDbContext dbContext) : IOrderItemRepository
{
    public async Task Create(Model.OrderItem orderItem)
    {
        try
        {
            await dbContext.OrderItems.AddAsync(orderItem);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception exception)
        {
            throw new Exception(exception.Message);
        }
    }
    
    public async Task<IEnumerable<Model.OrderItem>> Read()
    {
        try
        {
            return await dbContext.OrderItems.AsNoTracking().ToListAsync();
        }
        catch (Exception exception)
        {
            throw new Exception(exception.Message);
        }
    }

    public async Task Update(Model.OrderItem orderItem)
    {
        try
        {
            dbContext.OrderItems.Update(orderItem);
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException dbUpdateException)
        {
            throw new DbUpdateException("Failed to update order item", dbUpdateException);
        }
        catch (Exception exception)
        {
            throw new Exception(exception.Message);
        }
    }

    public async Task Delete(Model.OrderItem orderItem)
    {
        try
        {
            dbContext.OrderItems.Remove(orderItem);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception exception)
        {
            throw new Exception(exception.Message);
        }
    }
}