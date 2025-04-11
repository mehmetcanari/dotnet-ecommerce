using ECommerce.Application.Interfaces.Repository;
using ECommerce.Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories.OrderItem;

public class OrderItemRepository(StoreDbContext dbContext) : IOrderItemRepository
{
    public async Task Create(Domain.Model.OrderItem orderItem)
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
    
    public async Task<IEnumerable<Domain.Model.OrderItem>> Read()
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

    public async Task Update(Domain.Model.OrderItem orderItem)
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

    public async Task Delete(Domain.Model.OrderItem orderItem)
    {
        try
        {
            dbContext.OrderItems.Remove(orderItem);
            await dbContext.SaveChangesAsync();
            Console.WriteLine("Order item deleted successfully");
        }
        catch (Exception exception)
        {
            throw new Exception(exception.Message);
        }
    }
}