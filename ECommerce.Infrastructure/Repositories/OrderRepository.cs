using Microsoft.EntityFrameworkCore;
using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Infrastructure.Context;
using ECommerce.Shared.Constants;

namespace ECommerce.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly StoreDbContext _context;

    public OrderRepository(StoreDbContext context)
    {
        _context = context;
    }

    public async Task<List<Order>> Read(int pageNumber = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        try
        {
            IQueryable<Order> query = _context.Orders;

            var orders = await query
            .AsNoTracking()
            .Include(o => o.BasketItems)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

            return orders;
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }
    
    public async Task<List<Order>> GetPendings(Guid accountId, CancellationToken cancellationToken = default)
    {
        try
        {
            IQueryable<Order> query = _context.Orders;

            var orders = await query
                .AsNoTracking()
                .Where(o => o.UserId == accountId && o.Status == OrderStatus.Pending)
                .ToListAsync(cancellationToken);

            return orders;
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }
    
    public async Task<Order> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Where(o => o.Id == id)
                .Include(o => o.BasketItems)
                .FirstOrDefaultAsync(cancellationToken);

            return order;
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }

    public async Task<Order> GetByUserId(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId)
                .FirstOrDefaultAsync(cancellationToken);

            return order;
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }
    
    public async Task<List<Order>> GetOrders(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            IQueryable<Order> query = _context.Orders;

            var orders = await query
                .AsNoTracking()
                .Include(o => o.BasketItems)
                .Where(o => o.UserId == userId)
                .Where(o => o.BasketItems.Any(oi => oi.IsOrdered))
                .OrderByDescending(o => o.CreatedOn)
                .ToListAsync(cancellationToken);

            return orders;
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }

    public async Task Create(Order order, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Orders.AddAsync(order, cancellationToken);
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
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
            throw new Exception(ErrorMessages.UnexpectedError, exception);
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
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }
}