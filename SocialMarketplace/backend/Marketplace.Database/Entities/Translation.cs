namespace Marketplace.Database.Entities;

public class Translation : BaseEntity
{
    public Guid LanguageId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Namespace { get; set; } // common, dashboard, marketplace, etc.
    public string? Context { get; set; }
    public bool IsApproved { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    
    // Navigation properties
    public virtual Language Language { get; set; } = null!;
}
