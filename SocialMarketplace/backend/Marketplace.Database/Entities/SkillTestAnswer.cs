namespace Marketplace.Database.Entities;

public class SkillTestAnswer : BaseEntity
{
    public Guid AttemptId { get; set; }
    public Guid QuestionId { get; set; }
    public string Answer { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int PointsEarned { get; set; }
    public int TimeTakenSeconds { get; set; }
    public DateTime AnsweredAt { get; set; }
    
    // Navigation properties
    public virtual SkillTestAttempt Attempt { get; set; } = null!;
    public virtual SkillTestQuestion Question { get; set; } = null!;
}
