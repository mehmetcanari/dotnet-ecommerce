using ECommerce.Application.Interfaces.Service;
using ECommerce.Infrastructure.DatabaseContext;

public class UnitOfWork : IUnitOfWork
{
    private readonly StoreDbContext _context;
    private readonly ILoggingService _logger;

    public UnitOfWork(StoreDbContext context, ILoggingService logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task BeginTransactionAsync()
    {
        await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        await _context.Database.CommitTransactionAsync();
    }

    public async Task RollbackTransaction()
    {
        try
        {
            await _context.Database.RollbackTransactionAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rolling back transaction: {Message}", ex.Message);
            throw;
        }
    }

    public async Task Commit()
    {
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving changes to the database: {Message}", ex.Message);
            throw;
        }
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}