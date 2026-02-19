namespace Marketplace.Database.Entities;

public class ReviewResponse : BaseEntity
{
    public Guid ReviewId { get; set; }
    public Guid ResponderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsHidden { get; set; }
    public string? HiddenReason { get; set; }
    
    // Navigation properties
    public virtual Review Review { get; set; } = null!;
    public virtual User Responder { get; set; } = null!;
}
