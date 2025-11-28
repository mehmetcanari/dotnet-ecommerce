namespace ECommerce.Domain.Abstract.Repository;

public interface IUnitOfWork : IDisposable
{
    Task Commit();
    Task RollbackTransactionAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
}