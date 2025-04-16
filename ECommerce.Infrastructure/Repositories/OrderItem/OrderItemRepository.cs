using ECommerce.Application.Interfaces.Repository;
using ECommerce.Application.Interfaces.Service;
using ECommerce.Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories.OrderItem;

public class OrderItemRepository : IOrderItemRepository
{
    private readonly StoreDbContext _context;
    private readonly ILoggingService _logger;

    public OrderItemRepository(StoreDbContext context, ILoggingService logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Create(Domain.Model.OrderItem orderItem)
    {
        try
        {
            await _context.OrderItems.AddAsync(orderItem);
            await _context.SaveChangesAsync();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unexpected error occurred");
            throw new Exception(exception.Message);
        }
    }
    
    public async Task<IEnumerable<Domain.Model.OrderItem>> Read()
    {
        try
        {
            return await _context.OrderItems.AsNoTracking().ToListAsync();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unexpected error occurred");
            throw new Exception(exception.Message);
        }
    }

    public async Task Update(Domain.Model.OrderItem orderItem)
    {
        try
        {
            _context.OrderItems.Update(orderItem);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException dbUpdateException)
        {
            _logger.LogError(dbUpdateException, "Failed to update order item");
            throw new DbUpdateException("Failed to update order item", dbUpdateException);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unexpected error occurred");
            throw new Exception(exception.Message);
        }
    }

    public async Task Delete(Domain.Model.OrderItem orderItem)
    {
        try
        {
            _context.OrderItems.Remove(orderItem);
            await _context.SaveChangesAsync();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unexpected error occurred");
            throw new Exception(exception.Message);
        }
    }
}