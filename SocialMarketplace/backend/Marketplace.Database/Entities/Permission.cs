namespace Marketplace.Database.Entities;

public class Permission : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public string Module { get; set; } = string.Empty; // Users, Stores, Products, Orders, etc.
    public string Action { get; set; } = string.Empty; // Create, Read, Update, Delete, Manage
    public string? Resource { get; set; } // Specific resource type
    public bool IsSystemPermission { get; set; }
    
    // Navigation properties
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
