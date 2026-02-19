namespace Marketplace.Database.Entities;

public class ProductImage : BaseEntity
{
    public Guid ProductId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string? AltText { get; set; }
    public int SortOrder { get; set; }
    public bool IsPrimary { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public long? FileSize { get; set; }
    public string? MimeType { get; set; }
    
    // Navigation properties
    public virtual Product Product { get; set; } = null!;
}
