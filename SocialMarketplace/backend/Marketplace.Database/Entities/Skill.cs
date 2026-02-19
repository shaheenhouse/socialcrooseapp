namespace Marketplace.Database.Entities;

public class Skill : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }
    public string? IconUrl { get; set; }
    public bool IsVerifiable { get; set; }
    public bool HasTest { get; set; }
    public int? TestDurationMinutes { get; set; }
    public int? PassingScore { get; set; }
    public int UserCount { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public int SortOrder { get; set; }
    
    // Navigation properties
    public virtual Category? Category { get; set; }
    public virtual ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();
    public virtual ICollection<SkillTest> Tests { get; set; } = new List<SkillTest>();
}
