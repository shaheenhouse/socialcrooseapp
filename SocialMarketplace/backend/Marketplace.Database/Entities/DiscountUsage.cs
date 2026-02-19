namespace Marketplace.Database.Entities;

public class DiscountUsage : BaseEntity
{
    public Guid DiscountId { get; set; }
    public Guid UserId { get; set; }
    public Guid OrderId { get; set; }
    public decimal DiscountAmount { get; set; }
    
    // Navigation properties
    public virtual Discount Discount { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual Order Order { get; set; } = null!;
}
