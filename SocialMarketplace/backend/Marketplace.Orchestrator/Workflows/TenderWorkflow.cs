using Microsoft.Extensions.Logging;
using Marketplace.Core.Infrastructure;

namespace Marketplace.Orchestrator.Workflows;

/// <summary>
/// Government tender workflow - from publication to contract award
/// </summary>
public class TenderWorkflow : IWorkflow<TenderWorkflowInput, TenderWorkflowResult>
{
    private readonly IJobQueue _jobQueue;
    private readonly ILogger<TenderWorkflow> _logger;

    public string WorkflowId => "tender-workflow";
    public string WorkflowName => "Tender Processing Workflow";

    public TenderWorkflow(IJobQueue jobQueue, ILogger<TenderWorkflow> logger)
    {
        _jobQueue = jobQueue;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        await ExecuteAsync(new TenderWorkflowInput(), cancellationToken);
    }

    public async Task<TenderWorkflowResult> ExecuteAsync(TenderWorkflowInput input, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing tender workflow for {TenderId}, action: {Action}", input.TenderId, input.Action);

        try
        {
            switch (input.Action)
            {
                case TenderAction.Publish:
                    await HandleTenderPublicationAsync(input, cancellationToken);
                    break;
                case TenderAction.SubmitBid:
                    await HandleBidSubmissionAsync(input, cancellationToken);
                    break;
                case TenderAction.CloseSubmissions:
                    await HandleClosingSubmissionsAsync(input, cancellationToken);
                    break;
                case TenderAction.Evaluate:
                    await HandleEvaluationAsync(input, cancellationToken);
                    break;
                case TenderAction.AwardContract:
                    await HandleContractAwardAsync(input, cancellationToken);
                    break;
            }

            return new TenderWorkflowResult { Success = true, TenderId = input.TenderId };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tender workflow failed for {TenderId}", input.TenderId);
            return new TenderWorkflowResult { Success = false, TenderId = input.TenderId, Error = ex.Message };
        }
    }

    private async Task HandleTenderPublicationAsync(TenderWorkflowInput input, CancellationToken ct)
    {
        // Index for search
        await _jobQueue.EnqueueAsync("search-indexing", new
        {
            Type = "tender",
            Id = input.TenderId,
            Action = "index"
        });

        // Notify eligible vendors
        await _jobQueue.EnqueueAsync("notifications", new
        {
            Type = "new_tender",
            TenderId = input.TenderId,
            Category = input.Category
        });

        // Schedule deadline reminders
        await _jobQueue.EnqueueAsync("scheduled-tasks", new
        {
            Type = "tender_deadline_reminder",
            TenderId = input.TenderId,
            Deadline = input.SubmissionDeadline
        });
    }

    private async Task HandleBidSubmissionAsync(TenderWorkflowInput input, CancellationToken ct)
    {
        // Validate bid documents
        await _jobQueue.EnqueueAsync("document-validation", new
        {
            Type = "tender_bid",
            BidId = input.BidId,
            TenderId = input.TenderId
        });

        // Acknowledge receipt
        await _jobQueue.EnqueueAsync("notifications", new
        {
            Type = "bid_received",
            UserId = input.BidderId,
            TenderId = input.TenderId,
            BidId = input.BidId
        });
    }

    private async Task HandleClosingSubmissionsAsync(TenderWorkflowInput input, CancellationToken ct)
    {
        // Notify all bidders
        await _jobQueue.EnqueueAsync("notifications", new
        {
            Type = "tender_closed",
            TenderId = input.TenderId
        });

        // Generate bid summary for evaluation
        await _jobQueue.EnqueueAsync("reports", new
        {
            Type = "tender_bid_summary",
            TenderId = input.TenderId
        });
    }

    private async Task HandleEvaluationAsync(TenderWorkflowInput input, CancellationToken ct)
    {
        // Create evaluation tasks
        await _jobQueue.EnqueueAsync("tender-evaluation", new
        {
            TenderId = input.TenderId,
            EvaluationCriteria = input.EvaluationCriteria
        });
    }

    private async Task HandleContractAwardAsync(TenderWorkflowInput input, CancellationToken ct)
    {
        // Notify winner
        await _jobQueue.EnqueueAsync("notifications", new
        {
            Type = "contract_awarded",
            UserId = input.WinnerId,
            TenderId = input.TenderId
        });

        // Notify other bidders
        await _jobQueue.EnqueueAsync("notifications", new
        {
            Type = "tender_awarded",
            TenderId = input.TenderId,
            ExcludeUserId = input.WinnerId
        });

        // Create contract
        await _jobQueue.EnqueueAsync("contracts", new
        {
            Action = "create",
            TenderId = input.TenderId,
            VendorId = input.WinnerId,
            Amount = input.ContractAmount
        });

        // Publish award notice
        await _jobQueue.EnqueueAsync("public-notices", new
        {
            Type = "contract_award",
            TenderId = input.TenderId
        });
    }
}

public enum TenderAction
{
    Publish,
    SubmitBid,
    CloseSubmissions,
    Evaluate,
    AwardContract
}

public record TenderWorkflowInput
{
    public Guid TenderId { get; init; }
    public Guid BidId { get; init; }
    public Guid BidderId { get; init; }
    public Guid WinnerId { get; init; }
    public TenderAction Action { get; init; }
    public string Category { get; init; } = string.Empty;
    public DateTime SubmissionDeadline { get; init; }
    public string[] EvaluationCriteria { get; init; } = Array.Empty<string>();
    public decimal ContractAmount { get; init; }
}

public record TenderWorkflowResult
{
    public bool Success { get; init; }
    public Guid TenderId { get; init; }
    public string? Error { get; init; }
}
