using ECommerce.Domain.Abstract.Repository;
using Microsoft.EntityFrameworkCore;
using ECommerce.Domain.Model;
using ECommerce.Infrastructure.Context;

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

    public async Task<List<BasketItem>> GetNonOrderedBasketItems(Account account)
    {
        try
        {
            IQueryable<BasketItem> query = _context.BasketItems;
        
            var items = await query
                .Where(b => b.AccountId == account.Id && b.IsOrdered == false)
                .AsNoTracking()
                .ToListAsync();
            
            return items;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unexpected error occurred");
            throw new Exception(exception.Message);
        }
    }
    
    public async Task<BasketItem?> GetSpecificAccountBasketItemWithId(int id, Account account)
    {
        try
        {
            var basketItem = await _context.BasketItems
                .AsNoTracking()
                .Where(b => b.BasketItemId == id && b.AccountId == account.Id)
                .FirstOrDefaultAsync();

            return basketItem;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An unexpected error occurred");
            throw new Exception(exception.Message);
        }
    }
    
    public async Task<List<BasketItem>?> GetNonOrderedBasketItemIncludeSpecificProduct(int productId)
    {
        try
        {
            IQueryable<BasketItem> query = _context.BasketItems;
        
            var items = await query
                .Where(b => b.ProductId == productId && b.IsOrdered == false)
                .AsNoTracking()
                .ToListAsync();
            
            return items;
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