using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Marketplace.Workers.Workers;

/// <summary>
/// Worker for processing notification jobs
/// </summary>
public class NotificationWorker : BaseWorker
{
    public NotificationWorker(IConnectionMultiplexer redis, ILogger<NotificationWorker> logger)
        : base(redis, logger, "notifications")
    {
    }

    protected override async Task ProcessMessageAsync(StreamEntry entry, CancellationToken cancellationToken)
    {
        var type = GetValue<string>(entry, "Type");
        var userId = GetValue<Guid>(entry, "UserId");

        Logger.LogInformation("Processing notification type: {Type} for user: {UserId}", type, userId);

        switch (type)
        {
            case "new_order":
                await ProcessNewOrderNotificationAsync(entry, cancellationToken);
                break;
            case "order_confirmed":
                await ProcessOrderConfirmedNotificationAsync(entry, cancellationToken);
                break;
            case "milestone_completed":
                await ProcessMilestoneCompletedNotificationAsync(entry, cancellationToken);
                break;
            case "new_message":
                await ProcessNewMessageNotificationAsync(entry, cancellationToken);
                break;
            case "new_connection":
                await ProcessNewConnectionNotificationAsync(entry, cancellationToken);
                break;
            case "connection_accepted":
                await ProcessConnectionAcceptedNotificationAsync(entry, cancellationToken);
                break;
            case "new_follower":
                await ProcessNewFollowerNotificationAsync(entry, cancellationToken);
                break;
            default:
                Logger.LogWarning("Unknown notification type: {Type}", type);
                break;
        }
    }

    private async Task ProcessNewOrderNotificationAsync(StreamEntry entry, CancellationToken ct)
    {
        var userId = GetValue<Guid>(entry, "UserId");
        var orderId = GetValue<Guid>(entry, "OrderId");

        // In production, this would send actual notifications
        Logger.LogInformation("Sending new order notification to {UserId} for order {OrderId}", userId, orderId);
        await Task.CompletedTask;
    }

    private async Task ProcessOrderConfirmedNotificationAsync(StreamEntry entry, CancellationToken ct)
    {
        var userId = GetValue<Guid>(entry, "UserId");
        var orderId = GetValue<Guid>(entry, "OrderId");

        Logger.LogInformation("Sending order confirmed notification to {UserId} for order {OrderId}", userId, orderId);
        await Task.CompletedTask;
    }

    private async Task ProcessMilestoneCompletedNotificationAsync(StreamEntry entry, CancellationToken ct)
    {
        var userId = GetValue<Guid>(entry, "UserId");
        var milestoneId = GetValue<Guid>(entry, "MilestoneId");

        Logger.LogInformation("Sending milestone completed notification to {UserId}", userId);
        await Task.CompletedTask;
    }

    private async Task ProcessNewMessageNotificationAsync(StreamEntry entry, CancellationToken ct)
    {
        var userId = GetValue<Guid>(entry, "UserId");
        var senderId = GetValue<Guid>(entry, "SenderId");

        Logger.LogInformation("Sending new message notification to {UserId} from {SenderId}", userId, senderId);
        await Task.CompletedTask;
    }

    private async Task ProcessNewConnectionNotificationAsync(StreamEntry entry, CancellationToken ct)
    {
        var userId = GetValue<Guid>(entry, "UserId");
        var requesterId = GetValue<Guid>(entry, "RequesterId");

        Logger.LogInformation("Sending new connection request notification to {UserId} from {RequesterId}", userId, requesterId);
        await Task.CompletedTask;
    }

    private async Task ProcessConnectionAcceptedNotificationAsync(StreamEntry entry, CancellationToken ct)
    {
        var userId = GetValue<Guid>(entry, "UserId");
        var acceptedBy = GetValue<Guid>(entry, "AcceptedBy");

        Logger.LogInformation("Sending connection accepted notification to {UserId}", userId);
        await Task.CompletedTask;
    }

    private async Task ProcessNewFollowerNotificationAsync(StreamEntry entry, CancellationToken ct)
    {
        var userId = GetValue<Guid>(entry, "UserId");
        var followerId = GetValue<Guid>(entry, "FollowerId");

        Logger.LogInformation("Sending new follower notification to {UserId} from {FollowerId}", userId, followerId);
        await Task.CompletedTask;
    }
}
