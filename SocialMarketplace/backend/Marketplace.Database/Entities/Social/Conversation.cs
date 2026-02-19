namespace Marketplace.Database.Entities.Social;

/// <summary>
/// Conversation/chat thread between users
/// </summary>
public class Conversation
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public ConversationType Type { get; set; }
    public Guid? ProjectId { get; set; } // Associated project if any
    public Guid? OrderId { get; set; } // Associated order if any
    public Guid? LastMessageId { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}

public enum ConversationType
{
    Direct = 0,
    Group = 1,
    Project = 2,
    Order = 3,
    Support = 4
}

/// <summary>
/// Participant in a conversation
/// </summary>
public class ConversationParticipant
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid UserId { get; set; }
    public ParticipantRole Role { get; set; }
    public Guid? LastReadMessageId { get; set; }
    public DateTime? LastReadAt { get; set; }
    public int UnreadCount { get; set; }
    public bool IsMuted { get; set; }
    public bool IsArchived { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
}

public enum ParticipantRole
{
    Member = 0,
    Admin = 1,
    Owner = 2
}
