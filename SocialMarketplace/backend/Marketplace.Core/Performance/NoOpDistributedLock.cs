namespace Marketplace.Core.Performance;

/// <summary>
/// In-process no-op lock for single-instance dev when Redis is unavailable.
/// </summary>
public sealed class NoOpDistributedLock : IDistributedLock
{
    public Task<IAsyncDisposable?> AcquireAsync(string resource, TimeSpan expiry, CancellationToken cancellationToken = default)
        => Task.FromResult<IAsyncDisposable?>(new NoOpLockHandle());

    public Task<bool> TryAcquireAsync(string resource, TimeSpan expiry) => Task.FromResult(true);

    public Task ReleaseAsync(string resource, string token) => Task.CompletedTask;

    private sealed class NoOpLockHandle : IAsyncDisposable
    {
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
