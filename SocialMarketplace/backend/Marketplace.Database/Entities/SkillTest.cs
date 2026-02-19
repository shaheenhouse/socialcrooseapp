namespace Marketplace.Database.Entities;

public class SkillTest : BaseEntity
{
    public Guid SkillId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DurationMinutes { get; set; }
    public int TotalQuestions { get; set; }
    public int PassingScore { get; set; }
    public int MaxAttempts { get; set; } = 3;
    public int CooldownDays { get; set; } = 7; // Days before retry
    public bool IsProctored { get; set; }
    public bool ShuffleQuestions { get; set; } = true;
    public bool ShuffleAnswers { get; set; } = true;
    public bool ShowResults { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public string? Instructions { get; set; }
    public string? Metadata { get; set; }
    
    // Navigation properties
    public virtual Skill Skill { get; set; } = null!;
    public virtual ICollection<SkillTestQuestion> Questions { get; set; } = new List<SkillTestQuestion>();
    public virtual ICollection<SkillTestAttempt> Attempts { get; set; } = new List<SkillTestAttempt>();
}
