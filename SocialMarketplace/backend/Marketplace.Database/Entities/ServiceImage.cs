namespace Marketplace.Database.Entities;

public class ServiceImage : BaseEntity
{
    public Guid ServiceId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string? AltText { get; set; }
    public int SortOrder { get; set; }
    public bool IsPrimary { get; set; }
    public string? Type { get; set; } // image, video
    
    // Navigation properties
    public virtual Service Service { get; set; } = null!;
}
