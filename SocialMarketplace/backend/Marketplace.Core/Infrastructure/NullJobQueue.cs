namespace Marketplace.Core.Infrastructure;

/// <summary>
/// Used when Redis is disabled (local dev). Jobs are not persisted.
/// </summary>
public sealed class NullJobQueue : IJobQueue
{
    public Task EnqueueAsync<T>(string queueName, T payload, TimeSpan? delay = null) => Task.CompletedTask;

    public Task<T?> DequeueAsync<T>(string queueName, string consumerGroup, string consumerId, CancellationToken cancellationToken = default) where T : class
        => Task.FromResult<T?>(null);

    public Task AcknowledgeAsync(string queueName, string consumerGroup, string messageId) => Task.CompletedTask;
}
