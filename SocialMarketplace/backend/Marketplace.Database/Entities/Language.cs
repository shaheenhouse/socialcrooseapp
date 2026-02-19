namespace Marketplace.Database.Entities;

public class Language : BaseEntity
{
    public string Code { get; set; } = string.Empty; // en, ur, ar, etc.
    public string Name { get; set; } = string.Empty;
    public string NativeName { get; set; } = string.Empty;
    public string? FlagUrl { get; set; }
    public bool IsRtl { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    
    // Navigation properties
    public virtual ICollection<Translation> Translations { get; set; } = new List<Translation>();
}
