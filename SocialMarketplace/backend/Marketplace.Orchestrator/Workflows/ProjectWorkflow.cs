using Microsoft.Extensions.Logging;
using Marketplace.Core.Infrastructure;

namespace Marketplace.Orchestrator.Workflows;

/// <summary>
/// Project lifecycle workflow - from creation to completion
/// </summary>
public class ProjectWorkflow : IWorkflow<ProjectWorkflowInput, ProjectWorkflowResult>
{
    private readonly IJobQueue _jobQueue;
    private readonly ILogger<ProjectWorkflow> _logger;

    public string WorkflowId => "project-workflow";
    public string WorkflowName => "Project Lifecycle Workflow";

    public ProjectWorkflow(IJobQueue jobQueue, ILogger<ProjectWorkflow> logger)
    {
        _jobQueue = jobQueue;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        await ExecuteAsync(new ProjectWorkflowInput(), cancellationToken);
    }

    public async Task<ProjectWorkflowResult> ExecuteAsync(ProjectWorkflowInput input, CancellationToken cancellationToken = default)
    {
        var context = new WorkflowContext();
        _logger.LogInformation("Starting project workflow for project {ProjectId}", input.ProjectId);

        try
        {
            switch (input.Action)
            {
                case ProjectAction.Create:
                    await HandleProjectCreationAsync(input, context, cancellationToken);
                    break;
                case ProjectAction.AwardBid:
                    await HandleBidAwardAsync(input, context, cancellationToken);
                    break;
                case ProjectAction.CompleteMilestone:
                    await HandleMilestoneCompletionAsync(input, context, cancellationToken);
                    break;
                case ProjectAction.Complete:
                    await HandleProjectCompletionAsync(input, context, cancellationToken);
                    break;
                case ProjectAction.Cancel:
                    await HandleProjectCancellationAsync(input, context, cancellationToken);
                    break;
            }

            return new ProjectWorkflowResult { Success = true, ProjectId = input.ProjectId };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Project workflow failed for {ProjectId}", input.ProjectId);
            return new ProjectWorkflowResult { Success = false, ProjectId = input.ProjectId, Error = ex.Message };
        }
    }

    private async Task HandleProjectCreationAsync(ProjectWorkflowInput input, WorkflowContext context, CancellationToken ct)
    {
        // Notify matching freelancers
        await _jobQueue.EnqueueAsync("matching", new
        {
            Type = "new_project",
            ProjectId = input.ProjectId,
            Skills = input.RequiredSkills,
            Budget = input.Budget
        });

        // Index for search
        await _jobQueue.EnqueueAsync("search-indexing", new
        {
            Type = "project",
            Id = input.ProjectId,
            Action = "index"
        });
    }

    private async Task HandleBidAwardAsync(ProjectWorkflowInput input, WorkflowContext context, CancellationToken ct)
    {
        // Notify awarded freelancer
        await _jobQueue.EnqueueAsync("notifications", new
        {
            Type = "bid_awarded",
            UserId = input.FreelancerId,
            ProjectId = input.ProjectId
        });

        // Create escrow
        await _jobQueue.EnqueueAsync("escrow", new
        {
            Action = "create",
            ProjectId = input.ProjectId,
            Amount = input.Budget,
            ClientId = input.ClientId,
            FreelancerId = input.FreelancerId
        });

        // Notify other bidders
        await _jobQueue.EnqueueAsync("notifications", new
        {
            Type = "project_awarded",
            ProjectId = input.ProjectId,
            ExcludeUserId = input.FreelancerId
        });
    }

    private async Task HandleMilestoneCompletionAsync(ProjectWorkflowInput input, WorkflowContext context, CancellationToken ct)
    {
        // Notify client for review
        await _jobQueue.EnqueueAsync("notifications", new
        {
            Type = "milestone_completed",
            UserId = input.ClientId,
            ProjectId = input.ProjectId,
            MilestoneId = input.MilestoneId
        });

        // Update project progress
        await _jobQueue.EnqueueAsync("project-updates", new
        {
            ProjectId = input.ProjectId,
            Action = "update_progress"
        });
    }

    private async Task HandleProjectCompletionAsync(ProjectWorkflowInput input, WorkflowContext context, CancellationToken ct)
    {
        // Release escrow
        await _jobQueue.EnqueueAsync("escrow", new
        {
            Action = "release",
            ProjectId = input.ProjectId
        });

        // Request reviews
        await _jobQueue.EnqueueAsync("notifications", new
        {
            Type = "review_request",
            ProjectId = input.ProjectId,
            ClientId = input.ClientId,
            FreelancerId = input.FreelancerId
        });

        // Update freelancer stats
        await _jobQueue.EnqueueAsync("stats-update", new
        {
            UserId = input.FreelancerId,
            Type = "project_completed"
        });
    }

    private async Task HandleProjectCancellationAsync(ProjectWorkflowInput input, WorkflowContext context, CancellationToken ct)
    {
        // Refund escrow if applicable
        await _jobQueue.EnqueueAsync("escrow", new
        {
            Action = "refund",
            ProjectId = input.ProjectId,
            Reason = input.CancellationReason
        });

        // Notify participants
        await _jobQueue.EnqueueAsync("notifications", new
        {
            Type = "project_cancelled",
            ProjectId = input.ProjectId
        });
    }
}

public enum ProjectAction
{
    Create,
    AwardBid,
    CompleteMilestone,
    Complete,
    Cancel
}

public record ProjectWorkflowInput
{
    public Guid ProjectId { get; init; }
    public Guid ClientId { get; init; }
    public Guid FreelancerId { get; init; }
    public Guid? MilestoneId { get; init; }
    public ProjectAction Action { get; init; }
    public decimal Budget { get; init; }
    public string[] RequiredSkills { get; init; } = Array.Empty<string>();
    public string? CancellationReason { get; init; }
}

public record ProjectWorkflowResult
{
    public bool Success { get; init; }
    public Guid ProjectId { get; init; }
    public string? Error { get; init; }
}
