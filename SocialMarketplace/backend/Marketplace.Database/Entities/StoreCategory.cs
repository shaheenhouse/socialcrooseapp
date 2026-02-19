namespace Marketplace.Database.Entities;

public class StoreCategory : BaseEntity
{
    public Guid StoreId { get; set; }
    public Guid CategoryId { get; set; }
    public bool IsPrimary { get; set; }
    
    // Navigation properties
    public virtual Store Store { get; set; } = null!;
    public virtual Category Category { get; set; } = null!;
}
