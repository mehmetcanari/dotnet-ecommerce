using ECommerce.Application.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
using ECommerce.Infrastructure.DatabaseContext;
using ECommerce.Application.Interfaces.Service;
using ECommerce.Domain.Model;

namespace ECommerce.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly StoreDbContext _context;
    private readonly ILoggingService _logger;

    public OrderRepository(StoreDbContext context, ILoggingService logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Order>> Read()
    {
        try
        {
            return await _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .ToListAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to fetch orders");
            throw new DbUpdateException("Failed to fetch orders", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred");
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task Create(Order order)
    {
        try
        {
            await _context.Orders.AddAsync(order);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unexpected error occurred");
            throw new Exception(exception.Message);
        }
    }

    public void Update(Order order)
    {
        try
        {
            _context.Orders.Update(order);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to update order status");
            throw new DbUpdateException("Failed to update order status", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred");
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public void Delete(Order order)
    {
        try
        {
            _context.Orders.Remove(order);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to delete order");
            throw new DbUpdateException("Failed to delete order", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred");
            throw new Exception("An unexpected error occurred", ex);
        }
    }

    public async Task<Order> GetOrderById(int id)
    {
        try
        {
            return await _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == id) ?? throw new Exception("Order not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred");
            throw new Exception("An unexpected error occurred", ex);
        }
    }
}