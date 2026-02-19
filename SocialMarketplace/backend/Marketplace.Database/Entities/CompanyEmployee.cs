namespace Marketplace.Database.Entities;

public class CompanyEmployee : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public string? Title { get; set; }
    public string? Department { get; set; }
    public DateTime? JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Permissions { get; set; } // JSON
    public bool CanManageStores { get; set; }
    public bool CanManageEmployees { get; set; }
    public bool CanManageProjects { get; set; }
    public bool CanManageFinances { get; set; }
    
    // Navigation properties
    public virtual Company Company { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
}
