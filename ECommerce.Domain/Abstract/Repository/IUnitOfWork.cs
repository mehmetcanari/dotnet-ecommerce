namespace ECommerce.Domain.Abstract.Repository;

public interface IUnitOfWork : IDisposable
{
    Task Commit(); 
    Task RollbackTransaction();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
}