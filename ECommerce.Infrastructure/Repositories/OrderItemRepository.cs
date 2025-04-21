using ECommerce.Application.Interfaces.Repository;
using ECommerce.Application.Interfaces.Service;
using ECommerce.Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using ECommerce.Domain.Model;

namespace ECommerce.Infrastructure.Repositories;

public class OrderItemRepository : IOrderItemRepository
{
    private readonly StoreDbContext _context;
    private readonly ILoggingService _logger;

    public OrderItemRepository(StoreDbContext context, ILoggingService logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Create(OrderItem orderItem)
    {
        try
        {
            await _context.OrderItems.AddAsync(orderItem);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unexpected error occurred");
            throw new Exception(exception.Message);
        }
    }
    
    public async Task<IEnumerable<OrderItem>> Read()
    {
        try
        {
            return await _context.OrderItems
            .AsNoTracking()
            .ToListAsync();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unexpected error occurred");
            throw new Exception(exception.Message);
        }
    }

    public void Update(OrderItem orderItem)
    {
        try
        {
            _context.OrderItems.Update(orderItem);
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

    public void Delete(OrderItem orderItem)
    {
        try
        {
            _context.OrderItems.Remove(orderItem);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unexpected error occurred");
            throw new Exception(exception.Message);
        }
    }
}