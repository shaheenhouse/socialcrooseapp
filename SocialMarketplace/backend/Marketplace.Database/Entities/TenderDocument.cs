namespace Marketplace.Database.Entities;

public class TenderDocument : BaseEntity
{
    public Guid TenderId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? FileType { get; set; }
    public long? FileSize { get; set; }
    public string DocumentType { get; set; } = "General"; // General, TOR, Contract, Addendum
    public int SortOrder { get; set; }
    public bool IsPublic { get; set; } = true;
    public bool IsMandatory { get; set; }
    
    // Navigation properties
    public virtual Tender Tender { get; set; } = null!;
}
