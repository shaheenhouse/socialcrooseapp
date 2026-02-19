using Marketplace.Database.Enums;

namespace Marketplace.Database.Entities;

public class Store : BaseEntity
{
    public Guid OwnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }
    public StoreStatus Status { get; set; } = StoreStatus.Pending;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? BusinessRegistrationNumber { get; set; }
    public string? TaxId { get; set; }
    public decimal CommissionRate { get; set; } = 10m; // Default 10%
    public decimal Rating { get; set; }
    public int TotalReviews { get; set; }
    public int TotalProducts { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalSales { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public bool IsFeatured { get; set; }
    public string? SocialLinks { get; set; } // JSON
    public string? BusinessHours { get; set; } // JSON
    public string? ShippingPolicy { get; set; }
    public string? ReturnPolicy { get; set; }
    public string? Metadata { get; set; } // JSON for extensibility
    
    // Navigation properties
    public virtual User Owner { get; set; } = null!;
    public virtual ICollection<StoreEmployee> Employees { get; set; } = new List<StoreEmployee>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public virtual ICollection<StoreCategory> Categories { get; set; } = new List<StoreCategory>();
}
