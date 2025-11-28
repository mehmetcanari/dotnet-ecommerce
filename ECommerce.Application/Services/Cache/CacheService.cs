using ECommerce.Application.Abstract;
using ECommerce.Shared.Constants;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using ECommerce.Shared.Enum;

namespace ECommerce.Application.Services.Cache;

public class CacheService(IDistributedCache cache, ILogService logger) : ICacheService
{
    public async Task SetAsync<T>(string key, T value, CacheExpirationType expirationType, TimeSpan? expiry, CancellationToken cancellationToken)
    {
        try
        {
            var options = new DistributedCacheEntryOptions();
            switch (expirationType)
            {
                case CacheExpirationType.Absolute:
                    options.AbsoluteExpirationRelativeToNow = expiry;
                    break;

                case CacheExpirationType.Sliding:
                    options.SlidingExpiration = expiry;
                    break;
            }

            var json = JsonSerializer.Serialize(value);
            await cache.SetStringAsync(key, json, options, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.UnexpectedCacheError, key);
            throw;
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken)
    {
        try
        {
            await cache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, ErrorMessages.UnexpectedCacheError, key);
            throw;
        }
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
    {
        try
        {
            var value = await cache.GetStringAsync(key, cancellationToken);
            if (value is null)
                return default;

            var result = JsonSerializer.Deserialize<T>(value);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.UnexpectedCacheError, key);
            return default;
        }
    }
}
