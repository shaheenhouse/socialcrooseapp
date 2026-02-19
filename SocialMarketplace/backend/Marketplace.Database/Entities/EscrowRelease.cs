namespace Marketplace.Database.Entities;

public class EscrowRelease : BaseEntity
{
    public Guid EscrowId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string ReleaseType { get; set; } = "Full"; // Full, Partial
    public string? Notes { get; set; }
    public DateTime ReleasedAt { get; set; }
    public Guid ReleasedBy { get; set; }
    public Guid? TransactionId { get; set; }
    
    // Navigation properties
    public virtual Escrow Escrow { get; set; } = null!;
    public virtual Transaction? Transaction { get; set; }
}
