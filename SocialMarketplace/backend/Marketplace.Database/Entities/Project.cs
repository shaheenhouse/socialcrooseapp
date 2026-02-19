using Marketplace.Database.Enums;

namespace Marketplace.Database.Entities;

public class Project : BaseEntity
{
    public Guid ClientId { get; set; }
    public Guid? FreelancerId { get; set; }
    public Guid CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Requirements { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Draft;
    public string BudgetType { get; set; } = "Fixed"; // Fixed, Hourly, Range
    public decimal? BudgetMin { get; set; }
    public decimal? BudgetMax { get; set; }
    public decimal? AgreedBudget { get; set; }
    public string Currency { get; set; } = "USD";
    public int? EstimatedDurationDays { get; set; }
    public DateTime? Deadline { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? RequiredSkills { get; set; } // JSON array of skill IDs
    public string? ExperienceLevel { get; set; } // Entry, Intermediate, Expert
    public string? ProjectType { get; set; } // One-time, Ongoing, Contract
    public string? Visibility { get; set; } = "Public"; // Public, Private, Invite-only
    public int BidCount { get; set; }
    public int ViewCount { get; set; }
    public bool IsUrgent { get; set; }
    public bool IsFeatured { get; set; }
    public string? Attachments { get; set; } // JSON array of attachment URLs
    public string? Tags { get; set; }
    public decimal? Rating { get; set; }
    public string? Metadata { get; set; }
    
    // Navigation properties
    public virtual User Client { get; set; } = null!;
    public virtual User? Freelancer { get; set; }
    public virtual Category Category { get; set; } = null!;
    public virtual ICollection<ProjectBid> Bids { get; set; } = new List<ProjectBid>();
    public virtual ICollection<ProjectMilestone> Milestones { get; set; } = new List<ProjectMilestone>();
    public virtual ICollection<ProjectContract> Contracts { get; set; } = new List<ProjectContract>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public virtual ICollection<ChatRoom> ChatRooms { get; set; } = new List<ChatRoom>();
}
