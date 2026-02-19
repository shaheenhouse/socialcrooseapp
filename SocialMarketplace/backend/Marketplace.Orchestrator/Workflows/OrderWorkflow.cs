using Microsoft.Extensions.Logging;
using Marketplace.Core.Infrastructure;

namespace Marketplace.Orchestrator.Workflows;

/// <summary>
/// Order processing workflow - handles order creation to completion
/// </summary>
public class OrderWorkflow : IWorkflow<OrderWorkflowInput, OrderWorkflowResult>
{
    private readonly IJobQueue _jobQueue;
    private readonly ILogger<OrderWorkflow> _logger;

    public string WorkflowId => "order-workflow";
    public string WorkflowName => "Order Processing Workflow";

    public OrderWorkflow(IJobQueue jobQueue, ILogger<OrderWorkflow> logger)
    {
        _jobQueue = jobQueue;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        await ExecuteAsync(new OrderWorkflowInput(), cancellationToken);
    }

    public async Task<OrderWorkflowResult> ExecuteAsync(OrderWorkflowInput input, CancellationToken cancellationToken = default)
    {
        var context = new WorkflowContext();
        _logger.LogInformation("Starting order workflow {WorkflowId} for order {OrderId}", context.WorkflowId, input.OrderId);

        try
        {
            // Step 1: Validate order
            await ValidateOrderAsync(input, context, cancellationToken);

            // Step 2: Process payment (escrow)
            await ProcessPaymentAsync(input, context, cancellationToken);

            // Step 3: Notify seller
            await NotifySellerAsync(input, context, cancellationToken);

            // Step 4: Create project/delivery timeline
            await CreateDeliveryTimelineAsync(input, context, cancellationToken);

            // Step 5: Send confirmation to buyer
            await SendBuyerConfirmationAsync(input, context, cancellationToken);

            _logger.LogInformation("Order workflow {WorkflowId} completed successfully", context.WorkflowId);

            return new OrderWorkflowResult
            {
                Success = true,
                WorkflowId = context.WorkflowId,
                OrderId = input.OrderId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Order workflow {WorkflowId} failed", context.WorkflowId);

            // Compensate completed steps
            await CompensateAsync(context, cancellationToken);

            return new OrderWorkflowResult
            {
                Success = false,
                WorkflowId = context.WorkflowId,
                OrderId = input.OrderId,
                Error = ex.Message
            };
        }
    }

    private async Task ValidateOrderAsync(OrderWorkflowInput input, WorkflowContext context, CancellationToken ct)
    {
        _logger.LogDebug("Validating order {OrderId}", input.OrderId);
        // Validation logic here
        context.CompletedSteps.Add("ValidateOrder");
        await Task.CompletedTask;
    }

    private async Task ProcessPaymentAsync(OrderWorkflowInput input, WorkflowContext context, CancellationToken ct)
    {
        _logger.LogDebug("Processing payment for order {OrderId}", input.OrderId);
        // Queue payment processing job
        await _jobQueue.EnqueueAsync("payment-processing", new
        {
            OrderId = input.OrderId,
            Amount = input.Amount,
            BuyerId = input.BuyerId
        }, ct);
        context.CompletedSteps.Add("ProcessPayment");
    }

    private async Task NotifySellerAsync(OrderWorkflowInput input, WorkflowContext context, CancellationToken ct)
    {
        _logger.LogDebug("Notifying seller for order {OrderId}", input.OrderId);
        await _jobQueue.EnqueueAsync("notifications", new
        {
            Type = "new_order",
            UserId = input.SellerId,
            OrderId = input.OrderId
        }, ct);
        context.CompletedSteps.Add("NotifySeller");
    }

    private async Task CreateDeliveryTimelineAsync(OrderWorkflowInput input, WorkflowContext context, CancellationToken ct)
    {
        _logger.LogDebug("Creating delivery timeline for order {OrderId}", input.OrderId);
        // Create milestones, deadlines, etc.
        context.CompletedSteps.Add("CreateDeliveryTimeline");
        await Task.CompletedTask;
    }

    private async Task SendBuyerConfirmationAsync(OrderWorkflowInput input, WorkflowContext context, CancellationToken ct)
    {
        _logger.LogDebug("Sending buyer confirmation for order {OrderId}", input.OrderId);
        await _jobQueue.EnqueueAsync("notifications", new
        {
            Type = "order_confirmed",
            UserId = input.BuyerId,
            OrderId = input.OrderId
        }, ct);
        context.CompletedSteps.Add("SendBuyerConfirmation");
    }

    private async Task CompensateAsync(WorkflowContext context, CancellationToken ct)
    {
        _logger.LogWarning("Compensating workflow {WorkflowId}", context.WorkflowId);
        // Reverse completed steps in reverse order
        foreach (var step in context.CompletedSteps.AsEnumerable().Reverse())
        {
            _logger.LogDebug("Compensating step {StepName}", step);
            // Add compensation logic per step
        }
        await Task.CompletedTask;
    }
}

public record OrderWorkflowInput
{
    public Guid OrderId { get; init; }
    public Guid BuyerId { get; init; }
    public Guid SellerId { get; init; }
    public Guid ServiceId { get; init; }
    public decimal Amount { get; init; }
    public string PackageType { get; init; } = "basic";
}

public record OrderWorkflowResult
{
    public bool Success { get; init; }
    public string WorkflowId { get; init; } = string.Empty;
    public Guid OrderId { get; init; }
    public string? Error { get; init; }
}
