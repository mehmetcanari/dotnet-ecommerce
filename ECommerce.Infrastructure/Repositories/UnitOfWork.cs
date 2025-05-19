using ECommerce.Domain.Abstract.Repository;
using ECommerce.Infrastructure.Context;

namespace ECommerce.Infrastructure.Repositories;

public class UnitOfWork(StoreDbContext context) : IUnitOfWork
{
    public async Task BeginTransactionAsync()
    {
        await context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        await context.Database.CommitTransactionAsync();
    }

    public async Task RollbackTransaction()
    {
        try
        {
            await context.Database.RollbackTransactionAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred while rolling back transaction", ex);
        }
    }

    public async Task Commit()
    {
        try
        {
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred while committing changes", ex);
        }
    }

    public void Dispose()
    {
        context.Dispose();
    }
}