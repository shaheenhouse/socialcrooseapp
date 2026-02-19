using Marketplace.Database.Enums;

namespace Marketplace.Database.Entities;

public class ProductVariant : BaseEntity
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public string? Barcode { get; set; }
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public decimal? CostPrice { get; set; }
    public int StockQuantity { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.Active;
    public string? ImageUrl { get; set; }
    public decimal? Weight { get; set; }
    public string? Attributes { get; set; } // JSON: {"color": "red", "size": "XL"}
    public int SortOrder { get; set; }
    
    // Navigation properties
    public virtual Product Product { get; set; } = null!;
}
