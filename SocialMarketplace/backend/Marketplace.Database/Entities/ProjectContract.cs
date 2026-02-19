using Marketplace.Database.Enums;

namespace Marketplace.Database.Entities;

public class ProjectContract : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Guid ClientId { get; set; }
    public Guid FreelancerId { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Terms { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public ContractStatus Status { get; set; } = ContractStatus.Draft;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? ClientSignedAt { get; set; }
    public DateTime? FreelancerSignedAt { get; set; }
    public string? ClientSignature { get; set; }
    public string? FreelancerSignature { get; set; }
    public string? ContractDocUrl { get; set; }
    public bool AutoRenew { get; set; }
    public string? RenewalTerms { get; set; }
    public string? TerminationReason { get; set; }
    public DateTime? TerminatedAt { get; set; }
    public Guid? TerminatedBy { get; set; }
    
    // Navigation properties
    public virtual Project Project { get; set; } = null!;
    public virtual User Client { get; set; } = null!;
    public virtual User Freelancer { get; set; } = null!;
}
