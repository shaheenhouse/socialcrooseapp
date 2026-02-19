using Marketplace.Database.Enums;

namespace Marketplace.Database.Entities;

public class Escrow : BaseEntity
{
    public Guid? OrderId { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? MilestoneId { get; set; }
    public Guid BuyerId { get; set; }
    public Guid SellerId { get; set; }
    public string EscrowNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal ReleasedAmount { get; set; }
    public decimal RefundedAmount { get; set; }
    public decimal HeldAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public EscrowStatus Status { get; set; } = EscrowStatus.Pending;
    public DateTime? FundedAt { get; set; }
    public DateTime? ReleasedAt { get; set; }
    public DateTime? RefundedAt { get; set; }
    public string? ReleaseConditions { get; set; }
    public string? Notes { get; set; }
    public bool AutoRelease { get; set; }
    public int? AutoReleaseDays { get; set; }
    public DateTime? AutoReleaseDate { get; set; }
    public string? DisputeReason { get; set; }
    public DateTime? DisputedAt { get; set; }
    public Guid? DisputedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNotes { get; set; }
    public Guid? ResolvedBy { get; set; }
    
    // Navigation properties
    public virtual Order? Order { get; set; }
    public virtual Project? Project { get; set; }
    public virtual ProjectMilestone? Milestone { get; set; }
    public virtual User Buyer { get; set; } = null!;
    public virtual User Seller { get; set; } = null!;
    public virtual ICollection<EscrowRelease> Releases { get; set; } = new List<EscrowRelease>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
