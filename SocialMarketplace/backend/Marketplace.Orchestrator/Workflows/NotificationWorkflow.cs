using Microsoft.Extensions.Logging;
using Marketplace.Core.Infrastructure;

namespace Marketplace.Orchestrator.Workflows;

/// <summary>
/// Notification workflow - handles multi-channel notifications
/// </summary>
public class NotificationWorkflow : IWorkflow<NotificationWorkflowInput, NotificationWorkflowResult>
{
    private readonly IJobQueue _jobQueue;
    private readonly ILogger<NotificationWorkflow> _logger;

    public string WorkflowId => "notification-workflow";
    public string WorkflowName => "Notification Processing Workflow";

    public NotificationWorkflow(IJobQueue jobQueue, ILogger<NotificationWorkflow> logger)
    {
        _jobQueue = jobQueue;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        await ExecuteAsync(new NotificationWorkflowInput(), cancellationToken);
    }

    public async Task<NotificationWorkflowResult> ExecuteAsync(NotificationWorkflowInput input, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing notification for user {UserId}, type: {Type}", input.UserId, input.NotificationType);

        var results = new List<ChannelResult>();

        try
        {
            // In-app notification (always)
            if (input.Channels.HasFlag(NotificationChannel.InApp))
            {
                await SendInAppNotificationAsync(input, cancellationToken);
                results.Add(new ChannelResult(NotificationChannel.InApp, true));
            }

            // Email notification
            if (input.Channels.HasFlag(NotificationChannel.Email))
            {
                await SendEmailNotificationAsync(input, cancellationToken);
                results.Add(new ChannelResult(NotificationChannel.Email, true));
            }

            // Push notification
            if (input.Channels.HasFlag(NotificationChannel.Push))
            {
                await SendPushNotificationAsync(input, cancellationToken);
                results.Add(new ChannelResult(NotificationChannel.Push, true));
            }

            // SMS notification
            if (input.Channels.HasFlag(NotificationChannel.SMS))
            {
                await SendSmsNotificationAsync(input, cancellationToken);
                results.Add(new ChannelResult(NotificationChannel.SMS, true));
            }

            // Real-time (SignalR)
            if (input.Channels.HasFlag(NotificationChannel.Realtime))
            {
                await SendRealtimeNotificationAsync(input, cancellationToken);
                results.Add(new ChannelResult(NotificationChannel.Realtime, true));
            }

            return new NotificationWorkflowResult
            {
                Success = true,
                NotificationId = input.NotificationId,
                ChannelResults = results
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Notification workflow failed for {UserId}", input.UserId);
            return new NotificationWorkflowResult
            {
                Success = false,
                NotificationId = input.NotificationId,
                Error = ex.Message,
                ChannelResults = results
            };
        }
    }

    private async Task SendInAppNotificationAsync(NotificationWorkflowInput input, CancellationToken ct)
    {
        await _jobQueue.EnqueueAsync("inapp-notifications", new
        {
            UserId = input.UserId,
            Type = input.NotificationType,
            Title = input.Title,
            Body = input.Body,
            Data = input.Data,
            ActionUrl = input.ActionUrl
        });
    }

    private async Task SendEmailNotificationAsync(NotificationWorkflowInput input, CancellationToken ct)
    {
        await _jobQueue.EnqueueAsync("email", new
        {
            To = input.Email,
            Subject = input.Title,
            Template = input.EmailTemplate,
            Data = input.Data
        });
    }

    private async Task SendPushNotificationAsync(NotificationWorkflowInput input, CancellationToken ct)
    {
        await _jobQueue.EnqueueAsync("push-notifications", new
        {
            UserId = input.UserId,
            Title = input.Title,
            Body = input.Body,
            Data = input.Data
        });
    }

    private async Task SendSmsNotificationAsync(NotificationWorkflowInput input, CancellationToken ct)
    {
        await _jobQueue.EnqueueAsync("sms", new
        {
            Phone = input.Phone,
            Message = input.SmsMessage ?? input.Body
        });
    }

    private async Task SendRealtimeNotificationAsync(NotificationWorkflowInput input, CancellationToken ct)
    {
        await _jobQueue.EnqueueAsync("realtime-push", new
        {
            UserId = input.UserId,
            Event = "notification",
            Payload = new
            {
                Type = input.NotificationType,
                Title = input.Title,
                Body = input.Body,
                Data = input.Data,
                Timestamp = DateTime.UtcNow
            }
        });
    }
}

[Flags]
public enum NotificationChannel
{
    None = 0,
    InApp = 1,
    Email = 2,
    Push = 4,
    SMS = 8,
    Realtime = 16,
    All = InApp | Email | Push | SMS | Realtime
}

public record NotificationWorkflowInput
{
    public Guid NotificationId { get; init; } = Guid.NewGuid();
    public Guid UserId { get; init; }
    public string NotificationType { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? EmailTemplate { get; init; }
    public string? SmsMessage { get; init; }
    public string? ActionUrl { get; init; }
    public Dictionary<string, object> Data { get; init; } = new();
    public NotificationChannel Channels { get; init; } = NotificationChannel.InApp | NotificationChannel.Realtime;
}

public record ChannelResult(NotificationChannel Channel, bool Success, string? Error = null);

public record NotificationWorkflowResult
{
    public bool Success { get; init; }
    public Guid NotificationId { get; init; }
    public string? Error { get; init; }
    public List<ChannelResult> ChannelResults { get; init; } = new();
}
