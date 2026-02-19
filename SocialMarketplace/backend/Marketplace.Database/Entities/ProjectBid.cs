using Marketplace.Database.Enums;

namespace Marketplace.Database.Entities;

public class ProjectBid : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Guid FreelancerId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public int DeliveryDays { get; set; }
    public string? Proposal { get; set; }
    public BidStatus Status { get; set; } = BidStatus.Submitted;
    public bool IsShortlisted { get; set; }
    public DateTime? ShortlistedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }
    public string? Attachments { get; set; }
    public string? Milestones { get; set; } // JSON proposed milestones
    public decimal? ClientRating { get; set; }
    public string? ClientFeedback { get; set; }
    
    // Navigation properties
    public virtual Project Project { get; set; } = null!;
    public virtual User Freelancer { get; set; } = null!;
}
