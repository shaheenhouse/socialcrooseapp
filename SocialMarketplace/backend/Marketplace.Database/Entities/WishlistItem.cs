namespace Marketplace.Database.Entities;

public class WishlistItem : BaseEntity
{
    public Guid WishlistId { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? ServiceId { get; set; }
    public string ItemType { get; set; } = "Product"; // Product, Service
    public int Priority { get; set; }
    public string? Notes { get; set; }
    public decimal? PriceWhenAdded { get; set; }
    public bool NotifyOnPriceDrop { get; set; }
    public bool NotifyOnAvailability { get; set; }
    
    // Navigation properties
    public virtual Wishlist Wishlist { get; set; } = null!;
    public virtual Product? Product { get; set; }
    public virtual Service? Service { get; set; }
}
