using ECommerce.Domain.Abstract.Repository;
using ECommerce.Infrastructure.Context;
using Microsoft.EntityFrameworkCore.Storage;

namespace ECommerce.Infrastructure.Repositories;

public class StoreUnitOfWork(StoreDbContext storeContext) : IStoreUnitOfWork
{
    private IDbContextTransaction? _transaction;

    public async Task BeginTransactionAsync()
    {
        _transaction = await storeContext.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            if (_transaction == null)
                throw new InvalidOperationException("No transaction to commit");

            await storeContext.SaveChangesAsync();
            await _transaction.CommitAsync();
        }
        catch
        {
            await RollbackTransaction();
            throw;
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public async Task RollbackTransaction()
    {
        try
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
            }
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred while rolling back transaction", ex);
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public async Task Commit()
    {
        try
        {
            await storeContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred while committing changes", ex);
        }
    }

    private async Task DisposeTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        storeContext.Dispose();
    }
} 