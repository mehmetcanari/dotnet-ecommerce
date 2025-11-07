namespace ECommerce.Application.Abstract;

public interface ILockProvider
{
    Task<IDisposable> AcquireLockAsync(string key, CancellationToken cancellationToken);
}
