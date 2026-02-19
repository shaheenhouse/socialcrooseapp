namespace Marketplace.Database.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public string? ImageUrl { get; set; }
    public Guid? ParentId { get; set; }
    public int Level { get; set; }
    public string? Path { get; set; } // Materialized path for hierarchical queries
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public string? Type { get; set; } // Product, Service, Skill, Project
    public string? Metadata { get; set; }
    public int ProductCount { get; set; }
    public int ServiceCount { get; set; }
    
    // Navigation properties
    public virtual Category? Parent { get; set; }
    public virtual ICollection<Category> Children { get; set; } = new List<Category>();
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
    public virtual ICollection<Skill> Skills { get; set; } = new List<Skill>();
}
