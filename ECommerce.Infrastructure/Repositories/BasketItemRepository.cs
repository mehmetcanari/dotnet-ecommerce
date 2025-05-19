using ECommerce.Domain.Abstract.Repository;
using Microsoft.EntityFrameworkCore;
using ECommerce.Domain.Model;
using ECommerce.Infrastructure.Context;

namespace ECommerce.Infrastructure.Repositories;

public class BasketItemRepository : IBasketItemRepository
{
    private readonly StoreDbContext _context;

    public BasketItemRepository(StoreDbContext context)
    {
        _context = context;
    }

    public async Task Create(BasketItem basketItem)
    {
        try
        {
            await _context.BasketItems.AddAsync(basketItem);
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while creating basket item", exception);
        }
    }

    public async Task<List<BasketItem>> GetNonOrderedBasketItems(Account account)
    {
        try
        {
            IQueryable<BasketItem> query = _context.BasketItems;
        
            var items = await query
                .AsNoTracking()
                .Where(b => b.AccountId == account.Id && b.IsOrdered == false)
                .ToListAsync();
            
            return items;
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while getting non-ordered basket items", exception);
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
            throw new Exception("An unexpected error occurred while getting specific account basket item", exception);
        }
    }
    
    public async Task<List<BasketItem>?> GetNonOrderedBasketItemIncludeSpecificProduct(int productId)
    {
        try
        {
            IQueryable<BasketItem> query = _context.BasketItems;
        
            var items = await query
                .AsNoTracking()
                .Where(b => b.ProductId == productId && b.IsOrdered == false)
                .ToListAsync();
            
            return items;
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while getting non-ordered basket item with specific product", exception);
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
            throw new Exception("An unexpected error occurred while updating basket item", exception);
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
            throw new Exception("An unexpected error occurred while deleting basket item", exception);
        }
    }
}