using StackExchange.Redis;
using System.Text.Json;
using ECommerce.Shared.Constants;
using ECommerce.Application.Abstract;

namespace ECommerce.Application.Services.Cache;

public class CacheService(IConnectionMultiplexer redis, ILogService logger) : ICacheService
{
    private readonly IDatabase _database = redis.GetDatabase();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = null
    };

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            if (await _database.KeyExistsAsync(key))
            {
                var value = await _database.StringGetAsync(key);
                if (value.IsNull) 
                    return default;

                var result = JsonSerializer.Deserialize<T>(value!, _jsonOptions);
                return result;
            }
            return default;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.UnexpectedCacheError, key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            var serialized = JsonSerializer.Serialize(value, _jsonOptions);
            await _database.StringSetAsync(key, serialized, expiry);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.UnexpectedCacheError, key);
            throw;
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            if (await _database.KeyExistsAsync(key))
            {
                await _database.KeyDeleteAsync(key);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ErrorMessages.UnexpectedCacheError, key);
            throw;
        }
    }
}
