using Marketplace.Database.Enums;

namespace Marketplace.Database.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Pending;
    public bool EmailVerified { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }
    public bool PhoneVerified { get; set; }
    public DateTime? PhoneVerifiedAt { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public string? TwoFactorSecret { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? LastLoginIp { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public string PreferredLanguage { get; set; } = "en";
    public string? TimeZone { get; set; }
    public string? Currency { get; set; } = "USD";
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? PostalCode { get; set; }
    public decimal ReputationScore { get; set; }
    public int TotalReviews { get; set; }
    public decimal AverageRating { get; set; }
    public bool IsVerifiedSeller { get; set; }
    public bool IsVerifiedBuyer { get; set; }
    
    // Navigation properties
    public virtual UserProfile? Profile { get; set; }
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
    public virtual ICollection<UserSkill> Skills { get; set; } = new List<UserSkill>();
    public virtual ICollection<Store> Stores { get; set; } = new List<Store>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<Review> ReviewsGiven { get; set; } = new List<Review>();
    public virtual ICollection<Review> ReviewsReceived { get; set; } = new List<Review>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public virtual Wallet? Wallet { get; set; }
    public virtual ICollection<UserFeatureFlag> FeatureFlags { get; set; } = new List<UserFeatureFlag>();
    public virtual Portfolio? Portfolio { get; set; }
    public virtual ICollection<Design> Designs { get; set; } = new List<Design>();
    public virtual ICollection<Resume> Resumes { get; set; } = new List<Resume>();
}
