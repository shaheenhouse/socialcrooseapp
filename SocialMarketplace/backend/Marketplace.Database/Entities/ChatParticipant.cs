namespace Marketplace.Database.Entities;

public class ChatParticipant : BaseEntity
{
    public Guid ChatRoomId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = "Member"; // Admin, Moderator, Member
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    public DateTime? LastReadAt { get; set; }
    public Guid? LastReadMessageId { get; set; }
    public int UnreadCount { get; set; }
    public bool IsMuted { get; set; }
    public DateTime? MutedUntil { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Nickname { get; set; }
    
    // Navigation properties
    public virtual ChatRoom ChatRoom { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
