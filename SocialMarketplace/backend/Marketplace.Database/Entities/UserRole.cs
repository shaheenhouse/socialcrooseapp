namespace Marketplace.Database.Entities;

public class UserRole : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public Guid? GrantedBy { get; set; }
    public string? Notes { get; set; }
    
    // Scoped assignment
    public string? Scope { get; set; } // Store, Company, Agency, Project
    public Guid? ScopeEntityId { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
}
