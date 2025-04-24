using ECommerce.Application.Interfaces.Repository;
using ECommerce.Application.Interfaces.Service;
using ECommerce.Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using ECommerce.Domain.Model;

namespace ECommerce.Infrastructure.Repositories;

public class BasketItemRepository : IBasketItemRepository
{
    private readonly StoreDbContext _context;
    private readonly ILoggingService _logger;

    public BasketItemRepository(StoreDbContext context, ILoggingService logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Create(BasketItem basketItem)
    {
        try
        {
            await _context.BasketItems.AddAsync(basketItem);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unexpected error occurred");
            throw new Exception(exception.Message);
        }
    }
    
    public async Task<IEnumerable<BasketItem>> Read()
    {
        try
        {
            IQueryable<BasketItem> query = _context.BasketItems;

            var basketItems = await query
            .AsNoTracking()
            .ToListAsync();

            return basketItems;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unexpected error occurred");
            throw new Exception(exception.Message);
        }
    }

    public void Update(BasketItem basketItem)
    {
        try
        {
            _context.BasketItems.Update(basketItem);
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

    public void Delete(BasketItem basketItem)
    {
        try
        {
            _context.BasketItems.Remove(basketItem);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unexpected error occurred");
            throw new Exception(exception.Message);
        }
    }
}