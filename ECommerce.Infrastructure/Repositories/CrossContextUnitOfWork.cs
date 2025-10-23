using ECommerce.Domain.Abstract.Repository;
using ECommerce.Infrastructure.Context;
using ECommerce.Shared.Constants;
using Microsoft.EntityFrameworkCore.Storage;

namespace ECommerce.Infrastructure.Repositories;

public class CrossContextUnitOfWork(StoreDbContext storeContext, ApplicationIdentityDbContext identityContext) : ICrossContextUnitOfWork
{
    private IDbContextTransaction? _storeTransaction;
    private IDbContextTransaction? _identityTransaction;
    private bool _disposed;

    public async Task BeginTransactionAsync()
    {
        _storeTransaction = await storeContext.Database.BeginTransactionAsync();
        _identityTransaction = await identityContext.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            if (_storeTransaction == null || _identityTransaction == null)
                throw new InvalidOperationException(ErrorMessages.NoTransactionInProgress);

            await storeContext.SaveChangesAsync();
            await identityContext.SaveChangesAsync();
            
            await _storeTransaction.CommitAsync();
            await _identityTransaction.CommitAsync();
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
            if (_storeTransaction != null)
            {
                await _storeTransaction.RollbackAsync();
            }
            
            if (_identityTransaction != null)
            {
                await _identityTransaction.RollbackAsync();
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
            await storeContext.SaveChangesAsync();
            await identityContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception(ErrorMessages.UnexpectedError, ex);
        }
    }

    private async Task DisposeTransactionAsync()
    {
        if (_storeTransaction != null)
        {
            await _storeTransaction.DisposeAsync();
            _storeTransaction = null;
        }
        
        if (_identityTransaction != null)
        {
            await _identityTransaction.DisposeAsync();
            _identityTransaction = null;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _storeTransaction?.Dispose();
            _identityTransaction?.Dispose();
            _disposed = true;
        }
    }
}