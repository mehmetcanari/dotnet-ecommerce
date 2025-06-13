using StackExchange.Redis;
using System.Text.Json;
using ECommerce.Application.Abstract.Service;

namespace ECommerce.Application.Services.Cache;

public class RedisCacheService : ICacheService
{
    private readonly IDatabase _database;
    private readonly ILoggingService _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    
    public RedisCacheService(IConnectionMultiplexer redis, ILoggingService logger)
    {
        _database = redis.GetDatabase();
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = null // This ensures property names are preserved exactly as they are in the class
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
                {
                    _logger.LogWarning("Cache value for key: {Key} is null", key);
                    return default;
                }

                _logger.LogInformation("Retrieved value from cache for key {Key}: {Value}", key, value);
                var result = JsonSerializer.Deserialize<T>(value!, _jsonOptions);
                return result;
            }
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while getting cache value for key: {Key}", key);
            throw;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            var serialized = JsonSerializer.Serialize(value, _jsonOptions);
            _logger.LogInformation("Serialized value for key {Key}: {Value}", key, serialized);
            await _database.StringSetAsync(key, serialized, expiry);
            _logger.LogInformation("Cache value set for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while setting cache value for key: {Key}", key);
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
                _logger.LogInformation("Cache value removed for key: {Key}", key);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while removing cache value for key: {Key}", key);
            throw;
        }
    }
}
