namespace Marketplace.Orchestrator.Workflows;

/// <summary>
/// Base interface for all workflows
/// </summary>
public interface IWorkflow
{
    string WorkflowId { get; }
    string WorkflowName { get; }
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Generic workflow with input/output
/// </summary>
public interface IWorkflow<TInput, TOutput> : IWorkflow
{
    Task<TOutput> ExecuteAsync(TInput input, CancellationToken cancellationToken = default);
}

/// <summary>
/// Workflow step definition
/// </summary>
public interface IWorkflowStep<TInput, TOutput>
{
    string StepName { get; }
    Task<TOutput> ExecuteAsync(TInput input, CancellationToken cancellationToken = default);
    Task CompensateAsync(TInput input, CancellationToken cancellationToken = default);
}

/// <summary>
/// Workflow execution context
/// </summary>
public class WorkflowContext
{
    public string WorkflowId { get; init; } = Guid.NewGuid().ToString();
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
    public DateTime StartedAt { get; init; } = DateTime.UtcNow;
    public Dictionary<string, object> Data { get; } = new();
    public List<string> CompletedSteps { get; } = new();
    public List<WorkflowError> Errors { get; } = new();
}

public record WorkflowError(string StepName, string Message, Exception? Exception = null);
