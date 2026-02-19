using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Marketplace.Workers.Workers;

/// <summary>
/// Base worker class for background job processing
/// </summary>
public abstract class BaseWorker : BackgroundService
{
    protected readonly IConnectionMultiplexer Redis;
    protected readonly ILogger Logger;
    protected readonly string StreamName;
    protected readonly string ConsumerGroup;
    protected readonly string ConsumerName;

    protected BaseWorker(
        IConnectionMultiplexer redis,
        ILogger logger,
        string streamName)
    {
        Redis = redis;
        Logger = logger;
        StreamName = streamName;
        ConsumerGroup = $"{streamName}-group";
        ConsumerName = $"{streamName}-consumer-{Environment.MachineName}";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Logger.LogInformation("Worker {WorkerName} starting...", GetType().Name);

        await EnsureConsumerGroupExistsAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var db = Redis.GetDatabase();
                var entries = await db.StreamReadGroupAsync(
                    StreamName,
                    ConsumerGroup,
                    ConsumerName,
                    ">",
                    count: 10);

                foreach (var entry in entries)
                {
                    try
                    {
                        await ProcessMessageAsync(entry, stoppingToken);
                        await db.StreamAcknowledgeAsync(StreamName, ConsumerGroup, entry.Id);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Error processing message {MessageId}", entry.Id);
                        // Message will be retried since we didn't acknowledge it
                    }
                }

                if (entries.Length == 0)
                {
                    await Task.Delay(1000, stoppingToken); // Wait if no messages
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error in worker loop");
                await Task.Delay(5000, stoppingToken); // Wait before retry
            }
        }
    }

    private async Task EnsureConsumerGroupExistsAsync()
    {
        try
        {
            var db = Redis.GetDatabase();
            await db.StreamCreateConsumerGroupAsync(StreamName, ConsumerGroup, StreamPosition.NewMessages, true);
            Logger.LogInformation("Consumer group {ConsumerGroup} created for stream {StreamName}", ConsumerGroup, StreamName);
        }
        catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP"))
        {
            // Group already exists, which is fine
            Logger.LogDebug("Consumer group {ConsumerGroup} already exists", ConsumerGroup);
        }
    }

    protected abstract Task ProcessMessageAsync(StreamEntry entry, CancellationToken cancellationToken);

    protected T? GetValue<T>(StreamEntry entry, string key)
    {
        var value = entry.Values.FirstOrDefault(v => v.Name == key).Value;
        if (value.IsNullOrEmpty) return default;

        var stringValue = value.ToString();
        if (typeof(T) == typeof(string)) return (T)(object)stringValue;
        if (typeof(T) == typeof(Guid)) return (T)(object)Guid.Parse(stringValue);
        if (typeof(T) == typeof(int)) return (T)(object)int.Parse(stringValue);
        if (typeof(T) == typeof(decimal)) return (T)(object)decimal.Parse(stringValue);
        if (typeof(T) == typeof(bool)) return (T)(object)bool.Parse(stringValue);

        return System.Text.Json.JsonSerializer.Deserialize<T>(stringValue);
    }
}
