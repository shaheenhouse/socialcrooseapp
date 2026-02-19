namespace Marketplace.Database.Entities;

public class Review : BaseEntity
{
    public Guid ReviewerId { get; set; }
    public Guid? RevieweeId { get; set; }
    public Guid? OrderId { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? ServiceId { get; set; }
    public Guid? StoreId { get; set; }
    public Guid? ProjectId { get; set; }
    public string ReviewType { get; set; } = string.Empty; // Product, Service, Store, User, Project
    public int Rating { get; set; } // 1-5
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? Pros { get; set; }
    public string? Cons { get; set; }
    public bool IsVerifiedPurchase { get; set; }
    public bool IsRecommended { get; set; }
    public int HelpfulCount { get; set; }
    public int UnhelpfulCount { get; set; }
    public int ReportCount { get; set; }
    public bool IsFlagged { get; set; }
    public bool IsHidden { get; set; }
    public string? HiddenReason { get; set; }
    public bool HasResponse { get; set; }
    public string? Images { get; set; } // JSON array
    
    // Rating breakdown for detailed reviews
    public int? QualityRating { get; set; }
    public int? CommunicationRating { get; set; }
    public int? ValueRating { get; set; }
    public int? DeliveryRating { get; set; }
    public int? ProfessionalismRating { get; set; }
    
    // Navigation properties
    public virtual User Reviewer { get; set; } = null!;
    public virtual User? Reviewee { get; set; }
    public virtual Order? Order { get; set; }
    public virtual Product? Product { get; set; }
    public virtual Service? Service { get; set; }
    public virtual Store? Store { get; set; }
    public virtual Project? Project { get; set; }
    public virtual ReviewResponse? Response { get; set; }
}
