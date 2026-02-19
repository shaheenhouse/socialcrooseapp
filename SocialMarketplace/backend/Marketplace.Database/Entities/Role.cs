namespace Marketplace.Database.Entities;

public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
    public bool IsDefault { get; set; }
    public int Priority { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    
    // Scope - what entity type this role applies to
    public string? Scope { get; set; } // Global, Store, Company, Agency, Project
    public Guid? ScopeEntityId { get; set; } // If scoped to specific entity
    
    // Navigation properties
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public virtual ICollection<RoleFeatureFlag> FeatureFlags { get; set; } = new List<RoleFeatureFlag>();
}
