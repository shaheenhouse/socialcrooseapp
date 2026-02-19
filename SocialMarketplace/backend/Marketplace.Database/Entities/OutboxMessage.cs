namespace Marketplace.Database.Entities;

public class OutboxMessage : BaseEntity
{
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty; // JSON
    public string? AggregateType { get; set; }
    public Guid? AggregateId { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public bool IsProcessed { get; set; }
    public int RetryCount { get; set; }
    public DateTime? NextRetryAt { get; set; }
    public string? LastError { get; set; }
    public string? Metadata { get; set; }
}
