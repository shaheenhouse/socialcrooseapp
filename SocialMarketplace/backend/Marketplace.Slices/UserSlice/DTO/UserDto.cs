namespace Marketplace.Slices.UserSlice.DTO;

public record UserDto(
    Guid Id,
    string Email,
    string Username,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? AvatarUrl,
    string? Bio,
    string Status,
    bool EmailVerified,
    bool PhoneVerified,
    string PreferredLanguage,
    string? TimeZone,
    string? Currency,
    string? Country,
    string? City,
    decimal ReputationScore,
    decimal AverageRating,
    int TotalReviews,
    bool IsVerifiedSeller,
    bool IsVerifiedBuyer,
    DateTime CreatedAt);

public record UserProfileDto(
    Guid Id,
    Guid UserId,
    string? CompanyName,
    string? Website,
    string? LinkedInUrl,
    string? GitHubUrl,
    string? PortfolioUrl,
    string? Headline,
    string? About,
    int YearsOfExperience,
    decimal HourlyRate,
    bool AvailableForHire,
    int CompletedProjects,
    decimal TotalEarnings,
    bool IdVerified);

public record UserListDto(
    Guid Id,
    string Username,
    string FirstName,
    string LastName,
    string? AvatarUrl,
    string Status,
    decimal AverageRating,
    int TotalReviews,
    bool IsVerifiedSeller);

public record CreateUserDto(
    string Email,
    string Username,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber = null);

public record UpdateUserDto(
    string? FirstName = null,
    string? LastName = null,
    string? PhoneNumber = null,
    string? Bio = null,
    string? AvatarUrl = null,
    string? PreferredLanguage = null,
    string? TimeZone = null,
    string? Currency = null,
    string? Country = null,
    string? City = null,
    string? Address = null);

public record UpdateProfileDto(
    string? CompanyName = null,
    string? Website = null,
    string? LinkedInUrl = null,
    string? GitHubUrl = null,
    string? PortfolioUrl = null,
    string? Headline = null,
    string? About = null,
    int? YearsOfExperience = null,
    decimal? HourlyRate = null,
    bool? AvailableForHire = null);

public record UserSkillDto(
    Guid Id,
    Guid UserId,
    Guid SkillId,
    string SkillName,
    string Level,
    int YearsOfExperience,
    string VerificationStatus,
    int? TestScore,
    bool IsEndorsed,
    int EndorsementCount);
