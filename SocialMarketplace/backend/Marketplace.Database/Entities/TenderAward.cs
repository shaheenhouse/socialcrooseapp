namespace Marketplace.Database.Entities;

public class TenderAward : BaseEntity
{
    public Guid TenderId { get; set; }
    public Guid WinningBidId { get; set; }
    public Guid WinnerId { get; set; }
    public decimal AwardedAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime AwardedAt { get; set; }
    public string? AwardNotes { get; set; }
    public string? AwardDocUrl { get; set; }
    public DateTime? ContractSignedAt { get; set; }
    public string? ContractNumber { get; set; }
    public Guid? AwardedBy { get; set; }
    public bool PerformanceBondReceived { get; set; }
    public string? PerformanceBondDocUrl { get; set; }
    
    // Navigation properties
    public virtual Tender Tender { get; set; } = null!;
    public virtual TenderBid WinningBid { get; set; } = null!;
    public virtual User Winner { get; set; } = null!;
}
