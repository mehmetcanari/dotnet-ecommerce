using Microsoft.EntityFrameworkCore;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Infrastructure.Context;

namespace ECommerce.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly StoreDbContext _context;

    public OrderRepository(StoreDbContext context)
    {
        _context = context;
    }

    public async Task<List<Order>> Read(int pageNumber = 1, int pageSize = 50)
    {
        try
        {
            IQueryable<Order> query = _context.Orders;

            var orders = await query
            .AsNoTracking()
            .Include(o => o.BasketItems)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

            return orders;
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while reading orders", exception);
        }
    }
    
    public async Task<List<Order>> GetAccountPendingOrders(int accountId)
    {
        try
        {
            IQueryable<Order> query = _context.Orders;

            var orders = await query
                .AsNoTracking()
                .Where(o => o.AccountId == accountId && o.Status == OrderStatus.Pending)
                .ToListAsync();

            return orders;
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while getting account pending orders", exception);
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
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while retrieving order by id", exception);
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
        catch (Exception exception)
        {
            throw new Exception($"An unexpected error occurred while retrieving order by account id", exception);
        }
    }
    
    public async Task<List<Order?>> GetAccountOrders(int accountId)
    {
        try
        {
            IQueryable<Order> query = _context.Orders;

            var orders = await query
                .AsNoTracking()
                .Include(o => o.BasketItems)
                .Where(o => o.AccountId == accountId)
                .Where(o => o.BasketItems.Any(oi => oi.IsOrdered))
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return orders;
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while getting account orders", exception);
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
            throw new Exception("An unexpected error occurred while creating order", exception);
        }
    }

    public void Update(Order order)
    {
        try
        {
            _context.Orders.Update(order);
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while updating order", exception);
        }
    }

    public void Delete(Order order)
    {
        try
        {
            _context.Orders.Remove(order);
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while deleting order", exception);
        }
    }
}