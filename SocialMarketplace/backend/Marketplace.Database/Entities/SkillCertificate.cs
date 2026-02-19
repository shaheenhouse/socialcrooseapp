using Marketplace.Database.Enums;

namespace Marketplace.Database.Entities;

public class SkillCertificate : BaseEntity
{
    public Guid UserSkillId { get; set; }
    public Guid? TestAttemptId { get; set; }
    public string CertificateNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public VerificationStatus Status { get; set; } = VerificationStatus.Verified;
    public string? IssuerName { get; set; }
    public string? IssuerLogoUrl { get; set; }
    public string? CertificateUrl { get; set; }
    public string? VerificationUrl { get; set; }
    public string? BadgeUrl { get; set; }
    public int? Score { get; set; }
    public string? SkillLevel { get; set; }
    public string? Metadata { get; set; }
    
    // Navigation properties
    public virtual UserSkill UserSkill { get; set; } = null!;
    public virtual SkillTestAttempt? TestAttempt { get; set; }
}
