namespace Marketplace.Database.Entities;

public class UserProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public string? CompanyName { get; set; }
    public string? CompanyRegistrationNumber { get; set; }
    public string? TaxId { get; set; }
    public string? Website { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? GitHubUrl { get; set; }
    public string? PortfolioUrl { get; set; }
    public string? Headline { get; set; }
    public string? About { get; set; }
    public int YearsOfExperience { get; set; }
    public string? Education { get; set; }
    public string? Certifications { get; set; }
    public decimal HourlyRate { get; set; }
    public bool AvailableForHire { get; set; }
    public int CompletedProjects { get; set; }
    public int OngoingProjects { get; set; }
    public decimal TotalEarnings { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
    public string? BankRoutingNumber { get; set; }
    public string? PaypalEmail { get; set; }
    public string? StripeAccountId { get; set; }
    public bool IdVerified { get; set; }
    public DateTime? IdVerifiedAt { get; set; }
    public string? IdDocumentType { get; set; }
    public string? IdDocumentNumber { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
}
