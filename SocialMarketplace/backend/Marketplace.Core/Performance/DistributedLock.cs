using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Marketplace.Core.Performance;

public interface IDistributedLock
{
    Task<IAsyncDisposable?> AcquireAsync(string resource, TimeSpan expiry, CancellationToken cancellationToken = default);
    Task<bool> TryAcquireAsync(string resource, TimeSpan expiry);
    Task ReleaseAsync(string resource, string token);
}

public sealed class DistributedLock : IDistributedLock
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<DistributedLock> _logger;
    private const string LockPrefix = "lock:";

    public DistributedLock(IConnectionMultiplexer redis, ILogger<DistributedLock> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<IAsyncDisposable?> AcquireAsync(string resource, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        var token = Guid.NewGuid().ToString();
        var key = $"{LockPrefix}{resource}";
        var db = _redis.GetDatabase();

        var maxRetries = 10;
        var retryDelay = TimeSpan.FromMilliseconds(100);

        for (int i = 0; i < maxRetries; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (await db.StringSetAsync(key, token, expiry, When.NotExists))
            {
                _logger.LogDebug("Acquired lock for {Resource}", resource);
                return new LockHandle(this, resource, token);
            }

            await Task.Delay(retryDelay, cancellationToken);
            retryDelay = TimeSpan.FromMilliseconds(Math.Min(retryDelay.TotalMilliseconds * 2, 1000));
        }

        _logger.LogWarning("Failed to acquire lock for {Resource} after {Retries} retries", resource, maxRetries);
        return null;
    }

    public async Task<bool> TryAcquireAsync(string resource, TimeSpan expiry)
    {
        var token = Guid.NewGuid().ToString();
        var key = $"{LockPrefix}{resource}";
        var db = _redis.GetDatabase();

        return await db.StringSetAsync(key, token, expiry, When.NotExists);
    }

    public async Task ReleaseAsync(string resource, string token)
    {
        var key = $"{LockPrefix}{resource}";
        var db = _redis.GetDatabase();

        const string script = @"
            if redis.call('get', KEYS[1]) == ARGV[1] then
                return redis.call('del', KEYS[1])
            else
                return 0
            end";

        var result = (int)await db.ScriptEvaluateAsync(script, [key], [token]);

        if (result == 1)
        {
            _logger.LogDebug("Released lock for {Resource}", resource);
        }
        else
        {
            _logger.LogWarning("Failed to release lock for {Resource} - token mismatch or lock expired", resource);
        }
    }

    private sealed class LockHandle : IAsyncDisposable
    {
        private readonly DistributedLock _lock;
        private readonly string _resource;
        private readonly string _token;
        private bool _disposed;

        public LockHandle(DistributedLock @lock, string resource, string token)
        {
            _lock = @lock;
            _resource = resource;
            _token = token;
        }

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                _disposed = true;
                await _lock.ReleaseAsync(_resource, _token);
            }
        }
    }
}
