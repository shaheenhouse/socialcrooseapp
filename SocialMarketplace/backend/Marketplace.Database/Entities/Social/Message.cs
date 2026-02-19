namespace Marketplace.Database.Entities.Social;

/// <summary>
/// Direct message between users
/// </summary>
public class Message
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid SenderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public MessageType Type { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? AttachmentName { get; set; }
    public long? AttachmentSize { get; set; }
    public Guid? ReplyToId { get; set; }
    public bool IsEdited { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? EditedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}

public enum MessageType
{
    Text = 0,
    Image = 1,
    File = 2,
    Audio = 3,
    Video = 4,
    System = 5,
    Project = 6, // Project invitation/update
    Order = 7 // Order-related message
}
