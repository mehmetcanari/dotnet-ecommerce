using ECommerce.Application.Abstract;
using System.Collections.Concurrent;

namespace ECommerce.Application.Services.Lock
{
    public sealed class InMemoryLockProvider : ILockProvider
    {
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> Locks = new();

        public async Task<IDisposable> AcquireLockAsync(string key, CancellationToken cancellationToken = default)
        {
            var semaphore = Locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            return new Releaser(key, semaphore);
        }

        private sealed class Releaser(string key, SemaphoreSlim semaphore) : IDisposable
        {
            private bool _disposed;

            public void Dispose()
            {
                if (_disposed)
                    return;

                semaphore.Release();

                if (semaphore.CurrentCount == 1)
                    Locks.TryRemove(key, out _);

                _disposed = true;
            }
        }
    }
}