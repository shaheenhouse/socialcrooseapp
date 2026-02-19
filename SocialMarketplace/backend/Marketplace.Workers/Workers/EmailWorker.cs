using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Marketplace.Workers.Workers;

/// <summary>
/// Worker for processing email jobs
/// </summary>
public class EmailWorker : BaseWorker
{
    public EmailWorker(IConnectionMultiplexer redis, ILogger<EmailWorker> logger)
        : base(redis, logger, "email")
    {
    }

    protected override async Task ProcessMessageAsync(StreamEntry entry, CancellationToken cancellationToken)
    {
        var to = GetValue<string>(entry, "To");
        var subject = GetValue<string>(entry, "Subject");
        var template = GetValue<string>(entry, "Template");

        Logger.LogInformation("Processing email to: {To}, subject: {Subject}, template: {Template}", to, subject, template);

        // In production, integrate with email service (SendGrid, AWS SES, etc.)
        await SendEmailAsync(to!, subject!, template, entry, cancellationToken);
    }

    private async Task SendEmailAsync(string to, string subject, string? template, StreamEntry entry, CancellationToken ct)
    {
        // Simulate email sending
        Logger.LogInformation("Email sent to {To}: {Subject}", to, subject);
        await Task.Delay(100, ct); // Simulate network latency
    }
}
