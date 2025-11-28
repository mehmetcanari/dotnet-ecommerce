using ECommerce.Domain.Abstract.Repository;
using ECommerce.Domain.Model;
using ECommerce.Infrastructure.Context;
using ECommerce.Shared.Constants;
using Microsoft.EntityFrameworkCore;

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

    public async Task<List<BasketItem>> GetActiveItems(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            IQueryable<BasketItem> query = context.BasketItems;

            var items = await query
                .AsNoTracking()
                .Where(b => b.UserId == userId && b.IsPurchased == false)
                .ToListAsync(cancellationToken);

            return items;
        }
        catch (Exception exception)
        {
            throw new Exception(ErrorMessages.UnexpectedError, exception);
        }
    }

    public async Task<BasketItem?> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            IQueryable<BasketItem> query = context.BasketItems;

            var basketItem = await query
                .AsNoTracking()
                .Where(b => b.Id == id)
                .FirstOrDefaultAsync(cancellationToken);

            return basketItem;
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