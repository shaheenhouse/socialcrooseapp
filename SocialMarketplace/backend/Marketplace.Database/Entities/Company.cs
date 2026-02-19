using Marketplace.Database.Enums;

namespace Marketplace.Database.Entities;

public class Company : BaseEntity
{
    public Guid OwnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? TaxId { get; set; }
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }
    public string? Website { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string CompanyType { get; set; } = "Private"; // Private, Public, Government, NGO
    public string? Industry { get; set; }
    public int? FoundedYear { get; set; }
    public string? CompanySize { get; set; } // 1-10, 11-50, 51-200, etc.
    public UserStatus Status { get; set; } = UserStatus.Pending;
    public bool IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public decimal Rating { get; set; }
    public int TotalReviews { get; set; }
    public int TotalProjects { get; set; }
    public int TotalEmployees { get; set; }
    public string? SocialLinks { get; set; } // JSON
    public string? Metadata { get; set; }
    
    // Navigation properties
    public virtual User Owner { get; set; } = null!;
    public virtual ICollection<CompanyEmployee> Employees { get; set; } = new List<CompanyEmployee>();
    public virtual ICollection<Store> Stores { get; set; } = new List<Store>();
}
