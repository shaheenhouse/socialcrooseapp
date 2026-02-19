namespace Marketplace.Database.Entities;

public class SkillTestQuestion : BaseEntity
{
    public Guid TestId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string QuestionType { get; set; } = "MultipleChoice"; // MultipleChoice, TrueFalse, FillBlank, Code
    public string? Options { get; set; } // JSON array of options
    public string CorrectAnswer { get; set; } = string.Empty; // JSON for multiple answers
    public int Points { get; set; } = 1;
    public string? Explanation { get; set; }
    public string? Hint { get; set; }
    public int? TimeLimitSeconds { get; set; }
    public string? CodeLanguage { get; set; } // For code questions
    public string? TestCases { get; set; } // JSON for code questions
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Difficulty { get; set; } // Easy, Medium, Hard
    public string? Tags { get; set; }
    
    // Navigation properties
    public virtual SkillTest Test { get; set; } = null!;
}
