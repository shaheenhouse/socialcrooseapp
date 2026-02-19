using Marketplace.Database.Enums;

namespace Marketplace.Database.Entities;

public class Transaction : BaseEntity
{
    public Guid WalletId { get; set; }
    public Guid? PaymentId { get; set; }
    public Guid? OrderId { get; set; }
    public Guid? EscrowId { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public string Currency { get; set; } = "USD";
    public string? Description { get; set; }
    public string? Reference { get; set; }
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Completed;
    public string? Notes { get; set; }
    public string? Metadata { get; set; }
    
    // Navigation properties
    public virtual Wallet Wallet { get; set; } = null!;
    public virtual Payment? Payment { get; set; }
    public virtual Order? Order { get; set; }
    public virtual Escrow? Escrow { get; set; }
}
