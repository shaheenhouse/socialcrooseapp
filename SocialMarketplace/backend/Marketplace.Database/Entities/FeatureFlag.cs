namespace Marketplace.Database.Entities;

public class FeatureFlag : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsGlobal { get; set; } // Applies to all users
    public string? Module { get; set; } // Which module this feature belongs to
    public string? TargetAudience { get; set; } // All, Percentage, Specific
    public int? RolloutPercentage { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public string? Conditions { get; set; } // JSON conditions for complex targeting
    public string? Metadata { get; set; } // Additional JSON metadata
    
    // Navigation properties
    public virtual ICollection<UserFeatureFlag> UserFeatureFlags { get; set; } = new List<UserFeatureFlag>();
    public virtual ICollection<RoleFeatureFlag> RoleFeatureFlags { get; set; } = new List<RoleFeatureFlag>();
}
