namespace Marketplace.Database.Entities;

public class Resume : BaseEntity
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = "My Resume";
    public string Template { get; set; } = "modern";

    // JSON columns for flexible structured data
    public string PersonalInfo { get; set; } = "{}";
    public string Education { get; set; } = "[]";
    public string Experience { get; set; } = "[]";
    public string Skills { get; set; } = "[]";
    public string Certifications { get; set; } = "[]";
    public string Projects { get; set; } = "[]";
    public string Languages { get; set; } = "[]";
    public string CustomSections { get; set; } = "[]";
    public bool IsPublic { get; set; }
    public string? PdfUrl { get; set; }

    // Navigation
    public virtual User User { get; set; } = null!;
}
