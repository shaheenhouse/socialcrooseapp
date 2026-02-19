namespace Marketplace.Database.Entities;

public class RoleFeatureFlag : BaseEntity
{
    public Guid RoleId { get; set; }
    public Guid FeatureFlagId { get; set; }
    public bool IsEnabled { get; set; }
    
    // Navigation properties
    public virtual Role Role { get; set; } = null!;
    public virtual FeatureFlag FeatureFlag { get; set; } = null!;
}
