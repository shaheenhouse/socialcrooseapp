using Marketplace.Database.Enums;

namespace Marketplace.Database.Entities;

public class TenderBid : BaseEntity
{
    public Guid TenderId { get; set; }
    public Guid BidderId { get; set; } // User or Company
    public string BidderType { get; set; } = "Company"; // Individual, Company
    public string BidNumber { get; set; } = string.Empty;
    public decimal BidAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public BidStatus Status { get; set; } = BidStatus.Draft;
    public string? TechnicalProposal { get; set; }
    public string? FinancialProposal { get; set; }
    public string? ExecutionPlan { get; set; }
    public int? ProposedDurationDays { get; set; }
    public DateTime SubmittedAt { get; set; }
    public string? Documents { get; set; } // JSON array
    public decimal? TechnicalScore { get; set; }
    public decimal? FinancialScore { get; set; }
    public decimal? TotalScore { get; set; }
    public int? Rank { get; set; }
    public string? EvaluationNotes { get; set; }
    public DateTime? EvaluatedAt { get; set; }
    public Guid? EvaluatedBy { get; set; }
    public bool BidBondSubmitted { get; set; }
    public string? BidBondDocUrl { get; set; }
    public DateTime? BidBondExpiryDate { get; set; }
    
    // Navigation properties
    public virtual Tender Tender { get; set; } = null!;
    public virtual User Bidder { get; set; } = null!;
}
