using Marketplace.Database.Enums;

namespace Marketplace.Database.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Message { get; set; }
    public string? ImageUrl { get; set; }
    public string? ActionUrl { get; set; }
    public string? ActionType { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public bool IsSent { get; set; }
    public DateTime? SentAt { get; set; }
    public string? Channel { get; set; } // InApp, Email, Push, SMS
    public string? Metadata { get; set; }
    public int Priority { get; set; } // 1=Low, 2=Normal, 3=High, 4=Urgent
    public DateTime? ExpiresAt { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
}
