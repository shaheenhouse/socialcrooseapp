namespace Marketplace.Database.Entities;

public class Discount : BaseEntity
{
    public Guid? StoreId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DiscountType { get; set; } = "Percentage"; // Percentage, FixedAmount, FreeShipping
    public decimal Value { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public int? UsageLimit { get; set; }
    public int? UsageLimitPerUser { get; set; }
    public int UsageCount { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFirstOrderOnly { get; set; }
    public string? ApplicableProducts { get; set; } // JSON array of product IDs
    public string? ApplicableCategories { get; set; } // JSON array of category IDs
    public string? ExcludedProducts { get; set; } // JSON array
    public string? ApplicableUserRoles { get; set; } // JSON array
    public string? Metadata { get; set; }
    
    // Navigation properties
    public virtual Store? Store { get; set; }
    public virtual ICollection<DiscountUsage> Usages { get; set; } = new List<DiscountUsage>();
}
