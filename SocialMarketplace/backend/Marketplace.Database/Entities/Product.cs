using Marketplace.Database.Enums;

namespace Marketplace.Database.Entities;

public class Product : BaseEntity
{
    public Guid StoreId { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public string? Sku { get; set; }
    public string? Barcode { get; set; }
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public decimal? CostPrice { get; set; }
    public string Currency { get; set; } = "USD";
    public ProductStatus Status { get; set; } = ProductStatus.Draft;
    public int StockQuantity { get; set; }
    public int? LowStockThreshold { get; set; }
    public bool TrackInventory { get; set; } = true;
    public bool AllowBackorder { get; set; }
    public decimal? Weight { get; set; }
    public string? WeightUnit { get; set; }
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public string? DimensionUnit { get; set; }
    public string? Tags { get; set; }
    public decimal Rating { get; set; }
    public int TotalReviews { get; set; }
    public int TotalSold { get; set; }
    public int ViewCount { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsDigital { get; set; }
    public string? DigitalFileUrl { get; set; }
    public int? DownloadLimit { get; set; }
    public DateTime? DownloadExpiry { get; set; }
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }
    public string? SeoKeywords { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? Attributes { get; set; } // JSON for custom attributes
    public string? Metadata { get; set; }
    
    // Navigation properties
    public virtual Store Store { get; set; } = null!;
    public virtual Category Category { get; set; } = null!;
    public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public virtual ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
}
