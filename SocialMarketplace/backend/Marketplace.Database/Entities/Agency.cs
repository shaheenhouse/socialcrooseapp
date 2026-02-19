using Marketplace.Database.Enums;

namespace Marketplace.Database.Entities;

public class Agency : BaseEntity
{
    public Guid OwnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }
    public string AgencyType { get; set; } = "Freelance"; // Freelance, Recruitment, Marketing, Development
    public string? Specialization { get; set; }
    public string? Website { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Pending;
    public bool IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public decimal Rating { get; set; }
    public int TotalReviews { get; set; }
    public int TotalProjects { get; set; }
    public int TotalMembers { get; set; }
    public decimal CommissionRate { get; set; } = 15m;
    public string? Skills { get; set; } // JSON array
    public string? Portfolio { get; set; } // JSON array
    public string? Metadata { get; set; }
    
    // Navigation properties
    public virtual User Owner { get; set; } = null!;
    public virtual ICollection<AgencyMember> Members { get; set; } = new List<AgencyMember>();
}
