using Marketplace.Database.Enums;

namespace Marketplace.Database.Entities;

public class Order : BaseEntity
{
    public Guid BuyerId { get; set; }
    public Guid? StoreId { get; set; }
    public Guid? SellerId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string OrderType { get; set; } = "Product"; // Product, Service, Project
    public decimal Subtotal { get; set; }
    public decimal? DiscountAmount { get; set; }
    public string? DiscountCode { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal? ServiceFee { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal? PlatformCommission { get; set; }
    public decimal? SellerEarnings { get; set; }
    
    // Shipping
    public string? ShippingName { get; set; }
    public string? ShippingAddress { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingState { get; set; }
    public string? ShippingCountry { get; set; }
    public string? ShippingPostalCode { get; set; }
    public string? ShippingPhone { get; set; }
    public string? ShippingMethod { get; set; }
    public string? TrackingNumber { get; set; }
    public string? TrackingUrl { get; set; }
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    
    // Billing
    public string? BillingName { get; set; }
    public string? BillingAddress { get; set; }
    public string? BillingCity { get; set; }
    public string? BillingState { get; set; }
    public string? BillingCountry { get; set; }
    public string? BillingPostalCode { get; set; }
    
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }
    public Guid? CancelledBy { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsGift { get; set; }
    public string? GiftMessage { get; set; }
    public string? Metadata { get; set; }
    
    // Navigation properties
    public virtual User Buyer { get; set; } = null!;
    public virtual Store? Store { get; set; }
    public virtual User? Seller { get; set; }
    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public virtual Escrow? Escrow { get; set; }
}
