using Marketplace.Database.Enums;

namespace Marketplace.Database.Entities;

public class SkillTestAttempt : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid TestId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public TestStatus Status { get; set; } = TestStatus.NotStarted;
    public int Score { get; set; }
    public int TotalPoints { get; set; }
    public decimal Percentage { get; set; }
    public bool Passed { get; set; }
    public int QuestionsAnswered { get; set; }
    public int CorrectAnswers { get; set; }
    public int WrongAnswers { get; set; }
    public int TimeTakenSeconds { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool WasProctored { get; set; }
    public string? ProctoringData { get; set; } // JSON for proctoring flags
    public string? Answers { get; set; } // JSON of user answers
    public string? Notes { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual SkillTest Test { get; set; } = null!;
    public virtual ICollection<SkillTestAnswer> TestAnswers { get; set; } = new List<SkillTestAnswer>();
}
