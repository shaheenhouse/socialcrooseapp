namespace Marketplace.Database.Entities;

public class CartItem : BaseEntity
{
    public Guid CartId { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public Guid? ServiceId { get; set; }
    public Guid? ServicePackageId { get; set; }
    public string ItemType { get; set; } = "Product"; // Product, Service
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string? Attributes { get; set; } // JSON
    public string? Notes { get; set; }
    public bool SavedForLater { get; set; }
    
    // Navigation properties
    public virtual Cart Cart { get; set; } = null!;
    public virtual Product? Product { get; set; }
    public virtual ProductVariant? ProductVariant { get; set; }
    public virtual Service? Service { get; set; }
    public virtual ServicePackage? ServicePackage { get; set; }
}
