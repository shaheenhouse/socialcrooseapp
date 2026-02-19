namespace Marketplace.Database.Entities;

public class UserFeatureFlag : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid FeatureFlagId { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual FeatureFlag FeatureFlag { get; set; } = null!;
}
