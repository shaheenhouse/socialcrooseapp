namespace Marketplace.Database.Entities;

public class UserSession : BaseEntity
{
    public Guid UserId { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Location { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedReason { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
}
