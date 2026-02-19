namespace Marketplace.Database.Entities;

public class AuditLog : BaseEntity
{
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty; // Create, Update, Delete, Login, Logout, etc.
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON
    public string? ChangedProperties { get; set; } // JSON array
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? RequestPath { get; set; }
    public string? RequestMethod { get; set; }
    public int? StatusCode { get; set; }
    public long? DurationMs { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Metadata { get; set; }
    
    // Navigation properties
    public virtual User? User { get; set; }
}
