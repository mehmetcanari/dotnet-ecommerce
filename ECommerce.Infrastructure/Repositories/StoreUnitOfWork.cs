using ECommerce.Domain.Abstract.Repository;
using ECommerce.Infrastructure.Context;
using ECommerce.Shared.Constants;
using Microsoft.EntityFrameworkCore.Storage;

namespace ECommerce.Infrastructure.Repositories;

public class StoreUnitOfWork(StoreDbContext context) : IStoreUnitOfWork
{
    private IDbContextTransaction? _transaction;

    public async Task BeginTransactionAsync()
    {
        _transaction = await context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            if (_transaction == null)
                throw new InvalidOperationException(ErrorMessages.NoTransactionInProgress);

            await context.SaveChangesAsync();
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
            throw new Exception(ErrorMessages.UnexpectedError, ex);
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
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception(ErrorMessages.UnexpectedError, ex);
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
        context.Dispose();
    }
} 