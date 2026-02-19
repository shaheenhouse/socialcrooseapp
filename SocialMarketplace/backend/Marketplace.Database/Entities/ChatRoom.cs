namespace Marketplace.Database.Entities;

public class ChatRoom : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "Direct"; // Direct, Group, Project, Order, Support
    public Guid? ProjectId { get; set; }
    public Guid? OrderId { get; set; }
    public Guid? StoreId { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastMessageAt { get; set; }
    public Guid? LastMessageBy { get; set; }
    public string? LastMessagePreview { get; set; }
    public int UnreadCount { get; set; }
    public bool IsArchived { get; set; }
    public string? Metadata { get; set; }
    
    // Navigation properties
    public virtual Project? Project { get; set; }
    public virtual Order? Order { get; set; }
    public virtual Store? Store { get; set; }
    public virtual ICollection<ChatParticipant> Participants { get; set; } = new List<ChatParticipant>();
    public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
