namespace Marketplace.Database.Entities;

public class StoreEmployee : BaseEntity
{
    public Guid StoreId { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; } // Store-specific role
    public string? Title { get; set; }
    public string? Department { get; set; }
    public DateTime? JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Permissions { get; set; } // JSON for granular permissions
    
    // Navigation properties
    public virtual Store Store { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
}
