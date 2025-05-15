using Microsoft.EntityFrameworkCore;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Infrastructure.Context;

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
            IQueryable<Order> query = _context.Orders;

            var orders = await query
            .AsNoTracking()
            .Include(o => o.BasketItems)
            .ToListAsync();

            return orders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred");
            throw new Exception("An unexpected error occurred", ex);
        }
    }
    
    public async Task<List<Order>> GetAccountPendingOrders(int accountId)
    {
        try
        {
            IQueryable<Order> query = _context.Orders;

            var orders = await query
                .Where(o => o.AccountId == accountId && o.Status == OrderStatus.Pending)
                .AsNoTracking()
                .ToListAsync();

            return orders;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred");
            throw new Exception("An unexpected error occurred", ex);
        }
    }
    
    public async Task<Order?> GetOrderById(int id)
    {
        try
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Where(o => o.OrderId == id)
                .Include(o => o.BasketItems)
                .FirstOrDefaultAsync();

            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while retrieving order by id: {Id}", id);
            throw new Exception($"An unexpected error occurred while retrieving order by id: {id}", ex);
        }
    }

    public async Task<Order?> GetOrderByAccountId(int accountId)
    {
        try
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Where(o => o.AccountId == accountId)
                .FirstOrDefaultAsync();

            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while retrieving order by account id: {AccountId}", accountId);
            throw new Exception($"An unexpected error occurred while retrieving order by account id: {accountId}", ex);
        }
    }
    
    public async Task<List<Order?>> GetAccountOrders(int accountId)
    {
        try
        {
            IQueryable<Order> query = _context.Orders;

            var orders = await query
                .Where(o => o.AccountId == accountId)
                .Include(o => o.BasketItems)
                .AsNoTracking()
                .ToListAsync();

            return orders;
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred");
            throw new Exception("An unexpected error occurred", ex);
        }
    }
}