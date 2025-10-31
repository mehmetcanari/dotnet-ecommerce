using ECommerce.Domain.Model;

namespace ECommerce.Application.Abstract;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken);
    Task SetAsync<T>(string key, T value, CacheExpirationType expirationType, TimeSpan? expiry, CancellationToken cancellationToken);
    Task RemoveAsync(string key, CancellationToken cancellationToken);
}
