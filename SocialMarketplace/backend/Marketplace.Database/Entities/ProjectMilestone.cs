using Marketplace.Database.Enums;

namespace Marketplace.Database.Entities;

public class ProjectMilestone : BaseEntity
{
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime? DueDate { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public MilestoneStatus Status { get; set; } = MilestoneStatus.Pending;
    public int SortOrder { get; set; }
    public string? Deliverables { get; set; } // JSON array
    public string? SubmissionNotes { get; set; }
    public string? SubmissionAttachments { get; set; }
    public string? ReviewNotes { get; set; }
    public Guid? EscrowId { get; set; }
    public bool IsFunded { get; set; }
    public bool IsReleased { get; set; }
    
    // Navigation properties
    public virtual Project Project { get; set; } = null!;
    public virtual Escrow? Escrow { get; set; }
}
