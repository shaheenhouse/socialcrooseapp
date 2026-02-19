namespace Marketplace.Database.Entities.Social;

/// <summary>
/// Company/Organization page (like LinkedIn company pages)
/// </summary>
public class Page
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Tagline { get; set; }
    public string? LogoUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? Website { get; set; }
    public string? Industry { get; set; }
    public string? CompanySize { get; set; }
    public string? Headquarters { get; set; }
    public int? FoundedYear { get; set; }
    public PageType Type { get; set; }
    public Guid OwnerId { get; set; }
    public bool IsVerified { get; set; }
    public int FollowerCount { get; set; }
    public int EmployeeCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Social links
    public string? LinkedInUrl { get; set; }
    public string? TwitterUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public string? InstagramUrl { get; set; }
}

public enum PageType
{
    Company = 0,
    Organization = 1,
    Educational = 2,
    Government = 3,
    Community = 4,
    Brand = 5
}
