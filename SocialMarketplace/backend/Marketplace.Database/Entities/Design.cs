using Marketplace.Database.Enums;

namespace Marketplace.Database.Entities;

public class Design : BaseEntity
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = "Untitled Design";
    public string? Description { get; set; }
    public int Width { get; set; } = 1080;
    public int Height { get; set; } = 1080;
    public string CanvasJson { get; set; } = "{}";
    public string Thumbnail { get; set; } = string.Empty;
    public DesignStatus Status { get; set; } = DesignStatus.Draft;
    public string Category { get; set; } = "custom";
    public string? Tags { get; set; }
    public bool IsTemplate { get; set; }
    public bool IsPublic { get; set; }

    // Navigation
    public virtual User User { get; set; } = null!;
}
