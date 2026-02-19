namespace Marketplace.Database.Entities;

public class Wallet : BaseEntity
{
    public Guid UserId { get; set; }
    public decimal Balance { get; set; }
    public decimal PendingBalance { get; set; }
    public decimal HeldBalance { get; set; }
    public string Currency { get; set; } = "USD";
    public bool IsActive { get; set; } = true;
    public bool IsLocked { get; set; }
    public string? LockReason { get; set; }
    public DateTime? LockedAt { get; set; }
    public decimal TotalEarned { get; set; }
    public decimal TotalWithdrawn { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime? LastTransactionAt { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
