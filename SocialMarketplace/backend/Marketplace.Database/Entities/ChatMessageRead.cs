namespace Marketplace.Database.Entities;

public class ChatMessageRead : BaseEntity
{
    public Guid MessageId { get; set; }
    public Guid UserId { get; set; }
    public DateTime ReadAt { get; set; }
    
    // Navigation properties
    public virtual ChatMessage Message { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
