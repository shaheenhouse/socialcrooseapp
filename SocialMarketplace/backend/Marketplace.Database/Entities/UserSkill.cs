using Marketplace.Database.Enums;

namespace Marketplace.Database.Entities;

public class UserSkill : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid SkillId { get; set; }
    public SkillLevel Level { get; set; } = SkillLevel.Beginner;
    public int YearsOfExperience { get; set; }
    public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Unverified;
    public DateTime? VerifiedAt { get; set; }
    public int? TestScore { get; set; }
    public DateTime? LastTestedAt { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsEndorsed { get; set; }
    public int EndorsementCount { get; set; }
    public string? Description { get; set; }
    public string? PortfolioUrl { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Skill Skill { get; set; } = null!;
    public virtual ICollection<SkillCertificate> Certificates { get; set; } = new List<SkillCertificate>();
}
