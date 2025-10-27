using ECommerce.Domain.Abstract.Repository;
using Microsoft.EntityFrameworkCore;
using ECommerce.Domain.Model;
using ECommerce.Infrastructure.Context;
using ECommerce.Shared.Constants;

namespace ECommerce.Infrastructure.Repositories;

public class BasketItemRepository(StoreDbContext context) : IBasketItemRepository
{
    public async Task Create(BasketItem basketItem, CancellationToken cancellationToken = default)
    {
        try
        {
            await context.BasketItems.AddAsync(basketItem, cancellationToken);
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }

    public async Task<List<BasketItem>> GetUnorderedItems(User account, CancellationToken cancellationToken = default)
    {
        try
        {
            IQueryable<BasketItem> query = context.BasketItems;
        
            var items = await query
                .AsNoTracking()
                .Where(b => b.UserId == account.Id && b.IsPurchased == false)
                .ToListAsync(cancellationToken);
            
            return items;
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }
    
    public async Task<BasketItem?> GetUserCart(Guid id, User account, CancellationToken cancellationToken = default)
    {
        try
        {
            IQueryable<BasketItem> query = context.BasketItems;

            var basketItem = await query
                .AsNoTracking()
                .Where(b => b.Id == id && b.UserId == account.Id)
                .FirstOrDefaultAsync(cancellationToken);

            return basketItem;
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }
    
    public async Task<List<BasketItem>?> GetUnorderedByProductId(Guid productId, CancellationToken cancellationToken = default)
    {
        try
        {
            IQueryable<BasketItem> query = context.BasketItems;
        
            var items = await query
                .AsNoTracking()
                .Where(b => b.Id == productId && b.IsPurchased == false)
                .ToListAsync(cancellationToken);
            
            return items;
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }

    public void Update(BasketItem basketItem)
    {
        try
        {
            context.BasketItems.Update(basketItem);
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }

    public void Delete(BasketItem basketItem)
    {
        try
        {
            context.BasketItems.Remove(basketItem);
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }
}