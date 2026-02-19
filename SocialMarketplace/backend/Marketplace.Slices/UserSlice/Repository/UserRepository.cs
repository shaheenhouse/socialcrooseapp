using Dapper;
using Marketplace.Core.Infrastructure;
using Marketplace.Slices.UserSlice.DTO;

namespace Marketplace.Slices.UserSlice.Repository;

public class UserRepository : IUserRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public UserRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();
        
        const string sql = """
            SELECT id, email, username, first_name as FirstName, last_name as LastName,
                   phone_number as PhoneNumber, avatar_url as AvatarUrl, bio, status,
                   email_verified as EmailVerified, phone_verified as PhoneVerified,
                   preferred_language as PreferredLanguage, time_zone as TimeZone,
                   currency, country, city, reputation_score as ReputationScore,
                   average_rating as AverageRating, total_reviews as TotalReviews,
                   is_verified_seller as IsVerifiedSeller, is_verified_buyer as IsVerifiedBuyer,
                   created_at as CreatedAt
            FROM users
            WHERE id = @Id AND is_deleted = false
            """;
        
        return await connection.QuerySingleOrDefaultAsync<UserDto>(sql, new { Id = id });
    }

    public async Task<UserDto?> GetByEmailAsync(string email)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();
        
        const string sql = """
            SELECT id, email, username, first_name as FirstName, last_name as LastName,
                   phone_number as PhoneNumber, avatar_url as AvatarUrl, bio, status,
                   email_verified as EmailVerified, phone_verified as PhoneVerified,
                   preferred_language as PreferredLanguage, time_zone as TimeZone,
                   currency, country, city, reputation_score as ReputationScore,
                   average_rating as AverageRating, total_reviews as TotalReviews,
                   is_verified_seller as IsVerifiedSeller, is_verified_buyer as IsVerifiedBuyer,
                   created_at as CreatedAt
            FROM users
            WHERE LOWER(email) = LOWER(@Email) AND is_deleted = false
            """;
        
        return await connection.QuerySingleOrDefaultAsync<UserDto>(sql, new { Email = email });
    }

    public async Task<UserDto?> GetByUsernameAsync(string username)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();
        
        const string sql = """
            SELECT id, email, username, first_name as FirstName, last_name as LastName,
                   phone_number as PhoneNumber, avatar_url as AvatarUrl, bio, status,
                   email_verified as EmailVerified, phone_verified as PhoneVerified,
                   preferred_language as PreferredLanguage, time_zone as TimeZone,
                   currency, country, city, reputation_score as ReputationScore,
                   average_rating as AverageRating, total_reviews as TotalReviews,
                   is_verified_seller as IsVerifiedSeller, is_verified_buyer as IsVerifiedBuyer,
                   created_at as CreatedAt
            FROM users
            WHERE LOWER(username) = LOWER(@Username) AND is_deleted = false
            """;
        
        return await connection.QuerySingleOrDefaultAsync<UserDto>(sql, new { Username = username });
    }

    public async Task<(IEnumerable<UserListDto> Users, int TotalCount)> GetAllAsync(int page, int pageSize, string? search = null, string? status = null)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();
        
        var whereClause = "WHERE is_deleted = false";
        if (!string.IsNullOrEmpty(search))
        {
            whereClause += " AND (LOWER(username) LIKE LOWER(@Search) OR LOWER(first_name) LIKE LOWER(@Search) OR LOWER(last_name) LIKE LOWER(@Search) OR LOWER(email) LIKE LOWER(@Search))";
        }
        if (!string.IsNullOrEmpty(status))
        {
            whereClause += " AND status = @Status";
        }

        var countSql = $"SELECT COUNT(*) FROM users {whereClause}";
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, new { Search = $"%{search}%", Status = status });

        var sql = $"""
            SELECT id, username, first_name as FirstName, last_name as LastName,
                   avatar_url as AvatarUrl, status, average_rating as AverageRating,
                   total_reviews as TotalReviews, is_verified_seller as IsVerifiedSeller
            FROM users
            {whereClause}
            ORDER BY created_at DESC
            LIMIT @PageSize OFFSET @Offset
            """;

        var users = await connection.QueryAsync<UserListDto>(sql, new
        {
            Search = $"%{search}%",
            Status = status,
            PageSize = pageSize,
            Offset = (page - 1) * pageSize
        });

        return (users, totalCount);
    }

    public async Task<Guid> CreateAsync(CreateUserDto dto, string passwordHash)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();
        
        var id = Guid.NewGuid();
        const string sql = """
            INSERT INTO users (id, email, username, password_hash, first_name, last_name, phone_number,
                              status, created_at, is_deleted)
            VALUES (@Id, @Email, @Username, @PasswordHash, @FirstName, @LastName, @PhoneNumber,
                   1, NOW(), false)
            RETURNING id
            """;
        
        return await connection.ExecuteScalarAsync<Guid>(sql, new
        {
            Id = id,
            dto.Email,
            dto.Username,
            PasswordHash = passwordHash,
            dto.FirstName,
            dto.LastName,
            dto.PhoneNumber
        });
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateUserDto dto)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();
        
        var updates = new List<string>();
        var parameters = new DynamicParameters();
        parameters.Add("Id", id);

        if (dto.FirstName != null) { updates.Add("first_name = @FirstName"); parameters.Add("FirstName", dto.FirstName); }
        if (dto.LastName != null) { updates.Add("last_name = @LastName"); parameters.Add("LastName", dto.LastName); }
        if (dto.PhoneNumber != null) { updates.Add("phone_number = @PhoneNumber"); parameters.Add("PhoneNumber", dto.PhoneNumber); }
        if (dto.Bio != null) { updates.Add("bio = @Bio"); parameters.Add("Bio", dto.Bio); }
        if (dto.AvatarUrl != null) { updates.Add("avatar_url = @AvatarUrl"); parameters.Add("AvatarUrl", dto.AvatarUrl); }
        if (dto.PreferredLanguage != null) { updates.Add("preferred_language = @PreferredLanguage"); parameters.Add("PreferredLanguage", dto.PreferredLanguage); }
        if (dto.TimeZone != null) { updates.Add("time_zone = @TimeZone"); parameters.Add("TimeZone", dto.TimeZone); }
        if (dto.Currency != null) { updates.Add("currency = @Currency"); parameters.Add("Currency", dto.Currency); }
        if (dto.Country != null) { updates.Add("country = @Country"); parameters.Add("Country", dto.Country); }
        if (dto.City != null) { updates.Add("city = @City"); parameters.Add("City", dto.City); }
        if (dto.Address != null) { updates.Add("address = @Address"); parameters.Add("Address", dto.Address); }

        if (updates.Count == 0) return true;

        updates.Add("updated_at = NOW()");

        var sql = $"UPDATE users SET {string.Join(", ", updates)} WHERE id = @Id AND is_deleted = false";
        var rowsAffected = await connection.ExecuteAsync(sql, parameters);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();
        
        const string sql = "UPDATE users SET is_deleted = true, deleted_at = NOW() WHERE id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<bool> UpdateLastLoginAsync(Guid id, string ipAddress)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();
        
        const string sql = """
            UPDATE users
            SET last_login_at = NOW(), last_login_ip = @IpAddress, failed_login_attempts = 0
            WHERE id = @Id
            """;
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, IpAddress = ipAddress });
        return rowsAffected > 0;
    }

    public async Task<string?> GetPasswordHashAsync(Guid id)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();
        
        const string sql = "SELECT password_hash FROM users WHERE id = @Id AND is_deleted = false";
        return await connection.ExecuteScalarAsync<string?>(sql, new { Id = id });
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();
        
        const string sql = "SELECT EXISTS(SELECT 1 FROM users WHERE LOWER(email) = LOWER(@Email) AND is_deleted = false)";
        return await connection.ExecuteScalarAsync<bool>(sql, new { Email = email });
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();
        
        const string sql = "SELECT EXISTS(SELECT 1 FROM users WHERE LOWER(username) = LOWER(@Username) AND is_deleted = false)";
        return await connection.ExecuteScalarAsync<bool>(sql, new { Username = username });
    }

    public async Task<UserProfileDto?> GetProfileAsync(Guid userId)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();
        
        const string sql = """
            SELECT id, user_id as UserId, company_name as CompanyName, website,
                   linkedin_url as LinkedInUrl, github_url as GitHubUrl, portfolio_url as PortfolioUrl,
                   headline, about, years_of_experience as YearsOfExperience, hourly_rate as HourlyRate,
                   available_for_hire as AvailableForHire, completed_projects as CompletedProjects,
                   total_earnings as TotalEarnings, id_verified as IdVerified
            FROM user_profiles
            WHERE user_id = @UserId AND is_deleted = false
            """;
        
        return await connection.QuerySingleOrDefaultAsync<UserProfileDto>(sql, new { UserId = userId });
    }

    public async Task<bool> UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();
        
        // Upsert profile
        const string sql = """
            INSERT INTO user_profiles (id, user_id, company_name, website, linkedin_url, github_url,
                                       portfolio_url, headline, about, years_of_experience, hourly_rate,
                                       available_for_hire, created_at, is_deleted)
            VALUES (@Id, @UserId, @CompanyName, @Website, @LinkedInUrl, @GitHubUrl, @PortfolioUrl,
                   @Headline, @About, @YearsOfExperience, @HourlyRate, @AvailableForHire, NOW(), false)
            ON CONFLICT (user_id) 
            DO UPDATE SET
                company_name = COALESCE(@CompanyName, user_profiles.company_name),
                website = COALESCE(@Website, user_profiles.website),
                linkedin_url = COALESCE(@LinkedInUrl, user_profiles.linkedin_url),
                github_url = COALESCE(@GitHubUrl, user_profiles.github_url),
                portfolio_url = COALESCE(@PortfolioUrl, user_profiles.portfolio_url),
                headline = COALESCE(@Headline, user_profiles.headline),
                about = COALESCE(@About, user_profiles.about),
                years_of_experience = COALESCE(@YearsOfExperience, user_profiles.years_of_experience),
                hourly_rate = COALESCE(@HourlyRate, user_profiles.hourly_rate),
                available_for_hire = COALESCE(@AvailableForHire, user_profiles.available_for_hire),
                updated_at = NOW()
            """;
        
        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            dto.CompanyName,
            dto.Website,
            dto.LinkedInUrl,
            dto.GitHubUrl,
            dto.PortfolioUrl,
            dto.Headline,
            dto.About,
            dto.YearsOfExperience,
            dto.HourlyRate,
            dto.AvailableForHire
        });
        
        return rowsAffected > 0;
    }

    public async Task<IEnumerable<UserSkillDto>> GetUserSkillsAsync(Guid userId)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();
        
        const string sql = """
            SELECT us.id, us.user_id as UserId, us.skill_id as SkillId, s.name as SkillName,
                   us.level, us.years_of_experience as YearsOfExperience,
                   us.verification_status as VerificationStatus, us.test_score as TestScore,
                   us.is_endorsed as IsEndorsed, us.endorsement_count as EndorsementCount
            FROM user_skills us
            JOIN skills s ON us.skill_id = s.id
            WHERE us.user_id = @UserId AND us.is_deleted = false
            ORDER BY us.is_primary DESC, us.endorsement_count DESC
            """;
        
        return await connection.QueryAsync<UserSkillDto>(sql, new { UserId = userId });
    }

    public async Task<bool> AddUserSkillAsync(Guid userId, Guid skillId, string level, int yearsOfExperience)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();
        
        const string sql = """
            INSERT INTO user_skills (id, user_id, skill_id, level, years_of_experience, created_at, is_deleted)
            VALUES (@Id, @UserId, @SkillId, @Level, @YearsOfExperience, NOW(), false)
            ON CONFLICT (user_id, skill_id) DO UPDATE SET
                level = @Level,
                years_of_experience = @YearsOfExperience,
                updated_at = NOW()
            """;
        
        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SkillId = skillId,
            Level = level,
            YearsOfExperience = yearsOfExperience
        });
        
        return rowsAffected > 0;
    }

    public async Task<bool> RemoveUserSkillAsync(Guid userId, Guid skillId)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();
        
        const string sql = """
            UPDATE user_skills
            SET is_deleted = true, deleted_at = NOW()
            WHERE user_id = @UserId AND skill_id = @SkillId
            """;
        
        var rowsAffected = await connection.ExecuteAsync(sql, new { UserId = userId, SkillId = skillId });
        return rowsAffected > 0;
    }
}
