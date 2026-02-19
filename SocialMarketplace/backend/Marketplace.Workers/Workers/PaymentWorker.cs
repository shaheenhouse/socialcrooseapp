using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Marketplace.Workers.Workers;

/// <summary>
/// Worker for processing payment jobs
/// </summary>
public class PaymentWorker : BaseWorker
{
    public PaymentWorker(IConnectionMultiplexer redis, ILogger<PaymentWorker> logger)
        : base(redis, logger, "payment-processing")
    {
    }

    protected override async Task ProcessMessageAsync(StreamEntry entry, CancellationToken cancellationToken)
    {
        var action = GetValue<string>(entry, "Action");
        var userId = GetValue<Guid>(entry, "UserId");
        var amount = GetValue<decimal>(entry, "Amount");

        Logger.LogInformation("Processing payment: action={Action}, userId={UserId}, amount={Amount}", action, userId, amount);

        switch (action?.ToLower())
        {
            case "charge":
                await ProcessChargeAsync(entry, cancellationToken);
                break;
            case "payout":
                await ProcessPayoutAsync(entry, cancellationToken);
                break;
            case "refund":
                await ProcessRefundAsync(entry, cancellationToken);
                break;
            default:
                Logger.LogWarning("Unknown payment action: {Action}", action);
                break;
        }
    }

    private async Task ProcessChargeAsync(StreamEntry entry, CancellationToken ct)
    {
        var userId = GetValue<Guid>(entry, "UserId");
        var amount = GetValue<decimal>(entry, "Amount");
        var escrowId = GetValue<Guid>(entry, "EscrowId");

        // In production, integrate with payment gateway (Stripe, etc.)
        Logger.LogInformation("Charging {Amount} from user {UserId} for escrow {EscrowId}", amount, userId, escrowId);
        await Task.CompletedTask;
    }

    private async Task ProcessPayoutAsync(StreamEntry entry, CancellationToken ct)
    {
        var userId = GetValue<Guid>(entry, "UserId");
        var amount = GetValue<decimal>(entry, "Amount");

        // In production, process payout to seller's account
        Logger.LogInformation("Paying out {Amount} to user {UserId}", amount, userId);
        await Task.CompletedTask;
    }

    private async Task ProcessRefundAsync(StreamEntry entry, CancellationToken ct)
    {
        var userId = GetValue<Guid>(entry, "UserId");
        var amount = GetValue<decimal>(entry, "Amount");
        var reason = GetValue<string>(entry, "Reason");

        Logger.LogInformation("Refunding {Amount} to user {UserId}, reason: {Reason}", amount, userId, reason);
        await Task.CompletedTask;
    }
}
