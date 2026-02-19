namespace Marketplace.Database.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public Guid? ServiceId { get; set; }
    public Guid? ServicePackageId { get; set; }
    public string ItemType { get; set; } = "Product"; // Product, Service
    public string Name { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal TotalPrice { get; set; }
    public string Currency { get; set; } = "USD";
    public string? ImageUrl { get; set; }
    public string? Attributes { get; set; } // JSON for variant attributes
    public string? Notes { get; set; }
    public string? Requirements { get; set; } // For service orders
    public DateTime? DeliveryDate { get; set; }
    public bool IsDelivered { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public bool IsRefunded { get; set; }
    public decimal? RefundAmount { get; set; }
    
    // Navigation properties
    public virtual Order Order { get; set; } = null!;
    public virtual Product? Product { get; set; }
    public virtual ProductVariant? ProductVariant { get; set; }
    public virtual Service? Service { get; set; }
    public virtual ServicePackage? ServicePackage { get; set; }
}
