namespace Marketplace.Database.Entities;

public class Portfolio : BaseEntity
{
    public Guid UserId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public string Theme { get; set; } = "dark";

    // JSON columns for flexible structured data
    public string PersonalInfo { get; set; } = "{}";
    public string Education { get; set; } = "[]";
    public string Experience { get; set; } = "[]";
    public string Skills { get; set; } = "[]";
    public string Roles { get; set; } = "[]";
    public string Certifications { get; set; } = "[]";
    public string Projects { get; set; } = "[]";
    public string Achievements { get; set; } = "[]";
    public string Languages { get; set; } = "[]";
    public string Resumes { get; set; } = "[]";

    // Navigation
    public virtual User User { get; set; } = null!;
}
