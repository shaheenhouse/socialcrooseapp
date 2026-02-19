using Marketplace.Database.Enums;

namespace Marketplace.Database.Entities;

public class Service : BaseEntity
{
    public Guid SellerId { get; set; }
    public Guid? StoreId { get; set; }
    public Guid CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public decimal BasePrice { get; set; }
    public string Currency { get; set; } = "USD";
    public string PricingType { get; set; } = "Fixed"; // Fixed, Hourly, Daily, Custom
    public ProductStatus Status { get; set; } = ProductStatus.Draft;
    public int DeliveryTime { get; set; } // In days
    public string? DeliveryTimeUnit { get; set; } = "days";
    public int? Revisions { get; set; }
    public string? Tags { get; set; }
    public decimal Rating { get; set; }
    public int TotalReviews { get; set; }
    public int TotalOrders { get; set; }
    public int ViewCount { get; set; }
    public int QueueCount { get; set; }
    public bool IsFeatured { get; set; }
    public string? Requirements { get; set; }
    public string? Faq { get; set; } // JSON
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? Metadata { get; set; }
    
    // Navigation properties
    public virtual User Seller { get; set; } = null!;
    public virtual Store? Store { get; set; }
    public virtual Category Category { get; set; } = null!;
    public virtual ICollection<ServicePackage> Packages { get; set; } = new List<ServicePackage>();
    public virtual ICollection<ServiceImage> Images { get; set; } = new List<ServiceImage>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
