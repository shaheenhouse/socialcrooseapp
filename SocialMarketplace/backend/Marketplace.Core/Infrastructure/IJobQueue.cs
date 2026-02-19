namespace Marketplace.Core.Infrastructure;

public interface IJobQueue
{
    Task EnqueueAsync<T>(string queueName, T payload, TimeSpan? delay = null);
    Task<T?> DequeueAsync<T>(string queueName, string consumerGroup, string consumerId, CancellationToken cancellationToken = default) where T : class;
    Task AcknowledgeAsync(string queueName, string consumerGroup, string messageId);
}
