using Marketplace.Database.Enums;

namespace Marketplace.Database.Entities;

public class Payment : BaseEntity
{
    public Guid? OrderId { get; set; }
    public Guid? EscrowId { get; set; }
    public Guid PayerId { get; set; }
    public Guid? PayeeId { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string PaymentMethod { get; set; } = string.Empty; // Card, PayPal, Stripe, Bank, Wallet
    public string? PaymentGateway { get; set; }
    public string? GatewayTransactionId { get; set; }
    public string? GatewayResponse { get; set; } // JSON
    public string? PaymentIntentId { get; set; }
    public string? ChargeId { get; set; }
    public decimal? GatewayFee { get; set; }
    public decimal? PlatformFee { get; set; }
    public decimal? NetAmount { get; set; }
    public string? Description { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public string? FailureReason { get; set; }
    public string? FailureCode { get; set; }
    public bool IsRefund { get; set; }
    public Guid? OriginalPaymentId { get; set; }
    public string? RefundReason { get; set; }
    public DateTime? RefundedAt { get; set; }
    public string? IpAddress { get; set; }
    public string? BillingAddress { get; set; } // JSON
    public string? CardBrand { get; set; }
    public string? CardLast4 { get; set; }
    public string? ReceiptUrl { get; set; }
    public string? InvoiceUrl { get; set; }
    public string? Metadata { get; set; }
    
    // Navigation properties
    public virtual Order? Order { get; set; }
    public virtual Escrow? Escrow { get; set; }
    public virtual User Payer { get; set; } = null!;
    public virtual User? Payee { get; set; }
}
