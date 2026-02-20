using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Marketplace.Core.Caching;

public sealed class AdaptiveCache : IAdaptiveCache
{
    private readonly IMemoryCache _l1Cache;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<AdaptiveCache> _logger;
    private readonly TimeSpan _defaultExpiration;
    private readonly TimeSpan _l1Expiration;
    private readonly JsonSerializerOptions _jsonOptions;

    public AdaptiveCache(
        IMemoryCache l1Cache,
        IConnectionMultiplexer redis,
        IConfiguration configuration,
        ILogger<AdaptiveCache> logger)
    {
        _l1Cache = l1Cache;
        _redis = redis;
        _logger = logger;
        _defaultExpiration = TimeSpan.FromMinutes(configuration.GetValue("Cache:DefaultExpirationMinutes", 30));
        _l1Expiration = TimeSpan.FromSeconds(configuration.GetValue("Cache:L1ExpirationSeconds", 30));
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        // Try L1 (Memory) cache first
        if (_l1Cache.TryGetValue(key, out T? cachedValue))
        {
            return cachedValue;
        }

        // Try L2 (Redis) cache
        try
        {
            var db = _redis.GetDatabase();
            var value = await db.StringGetAsync(key);

            if (!value.IsNullOrEmpty)
            {
                var result = JsonSerializer.Deserialize<T>((string)value!, _jsonOptions);
                if (result != null)
                {
                    // Populate L1 cache
                    _l1Cache.Set(key, result, _l1Expiration);
                }
                return result;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis cache read failed for key {Key}", key);
        }

        return null;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        var exp = expiration ?? _defaultExpiration;
        var json = JsonSerializer.Serialize(value, _jsonOptions);

        // Set in L1 cache
        _l1Cache.Set(key, value, _l1Expiration);

        // Set in L2 cache
        try
        {
            var db = _redis.GetDatabase();
            await db.StringSetAsync(key, json, exp);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis cache write failed for key {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        _l1Cache.Remove(key);

        try
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis cache delete failed for key {Key}", key);
        }
    }

    public async Task RemoveByPrefixAsync(string prefix)
    {
        try
        {
            var endpoints = _redis.GetEndPoints();
            var server = _redis.GetServer(endpoints.First());
            var keys = server.Keys(pattern: $"{prefix}*");

            var db = _redis.GetDatabase();
            foreach (var key in keys)
            {
                _l1Cache.Remove(key.ToString());
                await db.KeyDeleteAsync(key);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis cache prefix delete failed for prefix {Prefix}", prefix);
        }
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class
    {
        var cached = await GetAsync<T>(key);
        if (cached != null)
        {
            return cached;
        }

        var value = await factory();
        await SetAsync(key, value, expiration);
        return value;
    }
}
