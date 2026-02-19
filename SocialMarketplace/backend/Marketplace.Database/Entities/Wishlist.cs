namespace Marketplace.Database.Entities;

public class Wishlist : BaseEntity
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = "My Wishlist";
    public string? Description { get; set; }
    public bool IsPublic { get; set; }
    public bool IsDefault { get; set; }
    public int ItemCount { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<WishlistItem> Items { get; set; } = new List<WishlistItem>();
}
