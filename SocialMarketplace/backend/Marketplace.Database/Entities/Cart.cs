namespace Marketplace.Database.Entities;

public class Cart : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid? StoreId { get; set; }
    public string? SessionId { get; set; } // For guest carts
    public decimal Subtotal { get; set; }
    public decimal? DiscountAmount { get; set; }
    public string? DiscountCode { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public int ItemCount { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsAbandoned { get; set; }
    public DateTime? AbandonedAt { get; set; }
    public bool ReminderSent { get; set; }
    public DateTime? ReminderSentAt { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Store? Store { get; set; }
    public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}
