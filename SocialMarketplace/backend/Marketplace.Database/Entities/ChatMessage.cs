namespace Marketplace.Database.Entities;

public class ChatMessage : BaseEntity
{
    public Guid ChatRoomId { get; set; }
    public Guid SenderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Type { get; set; } = "Text"; // Text, Image, File, System, Quote
    public Guid? ReplyToId { get; set; }
    public string? Attachments { get; set; } // JSON array
    public bool IsEdited { get; set; }
    public DateTime? EditedAt { get; set; }
    public bool IsPinned { get; set; }
    public DateTime? PinnedAt { get; set; }
    public Guid? PinnedBy { get; set; }
    public string? Reactions { get; set; } // JSON: {"👍": ["userId1", "userId2"]}
    public int ReactionCount { get; set; }
    public bool IsSystemMessage { get; set; }
    public string? Metadata { get; set; }
    
    // Navigation properties
    public virtual ChatRoom ChatRoom { get; set; } = null!;
    public virtual User Sender { get; set; } = null!;
    public virtual ChatMessage? ReplyTo { get; set; }
    public virtual ICollection<ChatMessageRead> ReadBy { get; set; } = new List<ChatMessageRead>();
}
