using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Marketplace.Workers.Workers;

/// <summary>
/// Worker for pushing real-time notifications via SignalR
/// </summary>
public class RealtimePushWorker : BaseWorker
{
    public RealtimePushWorker(IConnectionMultiplexer redis, ILogger<RealtimePushWorker> logger)
        : base(redis, logger, "realtime-push")
    {
    }

    protected override async Task ProcessMessageAsync(StreamEntry entry, CancellationToken cancellationToken)
    {
        var userId = GetValue<Guid>(entry, "UserId");
        var eventName = GetValue<string>(entry, "Event");
        var payload = GetValue<string>(entry, "Payload");

        Logger.LogInformation("Pushing realtime event {Event} to user {UserId}", eventName, userId);

        // In production, this would call SignalR hub
        await PushToUserAsync(userId, eventName!, payload!, cancellationToken);
    }

    private async Task PushToUserAsync(Guid userId, string eventName, string payload, CancellationToken ct)
    {
        // Use Redis pub/sub to broadcast to SignalR backplane
        var subscriber = Redis.GetSubscriber();
        await subscriber.PublishAsync(
            RedisChannel.Literal($"signalr:user:{userId}"),
            $"{{\"event\":\"{eventName}\",\"payload\":{payload}}}");
    }
}
