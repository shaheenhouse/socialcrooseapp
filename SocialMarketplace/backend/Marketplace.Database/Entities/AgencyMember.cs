namespace Marketplace.Database.Entities;

public class AgencyMember : BaseEntity
{
    public Guid AgencyId { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public string? Title { get; set; }
    public DateTime? JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal CommissionShare { get; set; } // Percentage of agency earnings
    public string? Permissions { get; set; } // JSON
    public bool CanBidOnProjects { get; set; }
    public bool CanManageMembers { get; set; }
    public bool CanManageFinances { get; set; }
    
    // Navigation properties
    public virtual Agency Agency { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
}
