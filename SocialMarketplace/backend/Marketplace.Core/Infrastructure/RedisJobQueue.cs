using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Marketplace.Core.Infrastructure;

public sealed class RedisJobQueue : IJobQueue
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisJobQueue> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisJobQueue(IConnectionMultiplexer redis, ILogger<RedisJobQueue> logger)
    {
        _redis = redis;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task EnqueueAsync<T>(string queueName, T payload, TimeSpan? delay = null)
    {
        try
        {
            var db = _redis.GetDatabase();
            var json = JsonSerializer.Serialize(payload, _jsonOptions);

            if (delay.HasValue && delay.Value > TimeSpan.Zero)
            {
                // Delayed job using sorted set
                var executeAt = DateTimeOffset.UtcNow.Add(delay.Value).ToUnixTimeMilliseconds();
                await db.SortedSetAddAsync($"{queueName}:delayed", json, executeAt);
            }
            else
            {
                // Immediate job using stream
                await db.StreamAddAsync(queueName, [new NameValueEntry("payload", json)]);
            }

            _logger.LogDebug("Enqueued job to {Queue}", queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enqueue job to {Queue}", queueName);
            throw;
        }
    }

    public async Task<T?> DequeueAsync<T>(string queueName, string consumerGroup, string consumerId, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var db = _redis.GetDatabase();

            // Ensure consumer group exists
            try
            {
                await db.StreamCreateConsumerGroupAsync(queueName, consumerGroup, StreamPosition.NewMessages, createStream: true);
            }
            catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP"))
            {
                // Group already exists, ignore
            }

            // Read from stream
            var entries = await db.StreamReadGroupAsync(
                queueName,
                consumerGroup,
                consumerId,
                ">",
                count: 1);

            if (entries.Length == 0)
            {
                return null;
            }

            var entry = entries[0];
            var payload = entry.Values.FirstOrDefault(v => v.Name == "payload").Value;

            if (payload.IsNullOrEmpty)
            {
                return null;
            }

            return JsonSerializer.Deserialize<T>((string)payload!, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to dequeue job from {Queue}", queueName);
            return null;
        }
    }

    public async Task AcknowledgeAsync(string queueName, string consumerGroup, string messageId)
    {
        try
        {
            var db = _redis.GetDatabase();
            await db.StreamAcknowledgeAsync(queueName, consumerGroup, messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to acknowledge message {MessageId} in {Queue}", messageId, queueName);
        }
    }
}
