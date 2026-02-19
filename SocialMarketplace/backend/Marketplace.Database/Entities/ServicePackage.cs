namespace Marketplace.Database.Entities;

public class ServicePackage : BaseEntity
{
    public Guid ServiceId { get; set; }
    public string Name { get; set; } = string.Empty; // Basic, Standard, Premium
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int DeliveryTime { get; set; }
    public int? Revisions { get; set; }
    public string? Features { get; set; } // JSON array of features
    public int SortOrder { get; set; }
    public bool IsPopular { get; set; }
    
    // Navigation properties
    public virtual Service Service { get; set; } = null!;
}
