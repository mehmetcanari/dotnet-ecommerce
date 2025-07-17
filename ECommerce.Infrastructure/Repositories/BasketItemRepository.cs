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

    public async Task Create(BasketItem basketItem, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.BasketItems.AddAsync(basketItem, cancellationToken);
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while creating basket item", exception);
        }
    }

    public async Task<List<BasketItem>> GetNonOrderedBasketItems(Account account, CancellationToken cancellationToken = default)
    {
        try
        {
            IQueryable<BasketItem> query = _context.BasketItems;
        
            var items = await query
                .AsNoTracking()
                .Where(b => b.AccountId == account.Id && b.IsOrdered == false)
                .ToListAsync(cancellationToken);
            
            return items;
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while getting non-ordered basket items", exception);
        }
    }
    
    public async Task<BasketItem?> GetSpecificAccountBasketItemWithId(int id, Account account, CancellationToken cancellationToken = default)
    {
        try
        {
            var basketItem = await _context.BasketItems
                .AsNoTracking()
                .Where(b => b.BasketItemId == id && b.AccountId == account.Id)
                .FirstOrDefaultAsync(cancellationToken);

            return basketItem;
        }
        catch (Exception exception)
        {
            throw new Exception("An unexpected error occurred while getting specific account basket item", exception);
        }
    }
    
    public async Task<List<BasketItem>?> GetNonOrderedBasketItemIncludeSpecificProduct(int productId, CancellationToken cancellationToken = default)
    {
        try
        {
            IQueryable<BasketItem> query = _context.BasketItems;
        
            var items = await query
                .AsNoTracking()
                .Where(b => b.ProductId == productId && b.IsOrdered == false)
                .ToListAsync(cancellationToken);
            
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