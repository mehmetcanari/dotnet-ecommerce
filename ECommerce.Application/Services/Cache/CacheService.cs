using StackExchange.Redis;
using System.Text.Json;
using ECommerce.Application.Abstract.Service;
using ECommerce.Shared.Constants;

namespace ECommerce.Application.Services.Cache;

public class CacheService : ICacheService
{
    private readonly IDatabase _database;
    private readonly ILoggingService _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    
    public CacheService(IConnectionMultiplexer redis, ILoggingService logger)
    {
        _database = redis.GetDatabase();
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = null
        };
    }

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
            _logger.LogError(ex, ErrorMessages.UnexpectedCacheError, key);
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
            _logger.LogError(ex, ErrorMessages.UnexpectedCacheError, key);
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
            _logger.LogError(ex, ErrorMessages.UnexpectedCacheError, key);
            throw;
        }
    }
}
