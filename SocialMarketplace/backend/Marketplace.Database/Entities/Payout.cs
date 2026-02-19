using Marketplace.Database.Enums;

namespace Marketplace.Database.Entities;

public class Payout : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid WalletId { get; set; }
    public string PayoutNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Fee { get; set; }
    public decimal NetAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string PayoutMethod { get; set; } = string.Empty; // BankTransfer, PayPal, Stripe
    public string? PayoutGateway { get; set; }
    public string? GatewayTransactionId { get; set; }
    public string? GatewayResponse { get; set; }
    
    // Bank details (if bank transfer)
    public string? BankName { get; set; }
    public string? AccountNumber { get; set; }
    public string? AccountHolderName { get; set; }
    public string? RoutingNumber { get; set; }
    public string? SwiftCode { get; set; }
    public string? Iban { get; set; }
    
    // Other payment methods
    public string? PaypalEmail { get; set; }
    public string? StripeAccountId { get; set; }
    
    public DateTime? RequestedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public string? FailureReason { get; set; }
    public string? Notes { get; set; }
    public Guid? ProcessedBy { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Wallet Wallet { get; set; } = null!;
}
