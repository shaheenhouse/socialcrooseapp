using Marketplace.Database.Enums;

namespace Marketplace.Database.Entities;

public class Tender : BaseEntity
{
    public Guid OrganizationId { get; set; } // Can be Company, Agency, or Government entity
    public string OrganizationType { get; set; } = string.Empty; // Company, Agency, Government
    public string TenderNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Scope { get; set; }
    public string? Requirements { get; set; }
    public string? Eligibility { get; set; }
    public TenderStatus Status { get; set; } = TenderStatus.Draft;
    public string TenderType { get; set; } = "Open"; // Open, Limited, Restricted
    public Guid CategoryId { get; set; }
    public decimal? EstimatedBudget { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime PublishDate { get; set; }
    public DateTime SubmissionDeadline { get; set; }
    public DateTime? OpeningDate { get; set; }
    public DateTime? AwardDate { get; set; }
    public DateTime? ProjectStartDate { get; set; }
    public DateTime? ProjectEndDate { get; set; }
    public string? Location { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Documents { get; set; } // JSON array of document URLs
    public decimal? DocumentFee { get; set; }
    public decimal? BidBond { get; set; }
    public decimal? PerformanceBond { get; set; }
    public string? EvaluationCriteria { get; set; } // JSON
    public int BidCount { get; set; }
    public int ViewCount { get; set; }
    public bool IsUrgent { get; set; }
    public bool IsFeatured { get; set; }
    public string? Metadata { get; set; }
    
    // Navigation properties
    public virtual Category Category { get; set; } = null!;
    public virtual ICollection<TenderBid> Bids { get; set; } = new List<TenderBid>();
    public virtual ICollection<TenderDocument> Documents2 { get; set; } = new List<TenderDocument>();
    public virtual TenderAward? Award { get; set; }
}
