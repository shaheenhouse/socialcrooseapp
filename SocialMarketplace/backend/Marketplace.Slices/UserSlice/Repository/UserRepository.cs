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
            SELECT "Id", "Email", "Username", "FirstName", "LastName",
                   "PhoneNumber", "AvatarUrl", "Bio", "Status",
                   "EmailVerified", "PhoneVerified",
                   "PreferredLanguage", "TimeZone",
                   "Currency", "Country", "City", "ReputationScore",
                   "AverageRating", "TotalReviews",
                   "IsVerifiedSeller", "IsVerifiedBuyer",
                   "CreatedAt"
            FROM users
            WHERE "Id" = @Id AND "IsDeleted" = false
            """;
        
        return await connection.QuerySingleOrDefaultAsync<UserDto>(sql, new { Id = id });
    }

    public async Task<UserDto?> GetByEmailAsync(string email)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();
        
        const string sql = """
            SELECT "Id", "Email", "Username", "FirstName", "LastName",
                   "PhoneNumber", "AvatarUrl", "Bio", "Status",
                   "EmailVerified", "PhoneVerified",
                   "PreferredLanguage", "TimeZone",
                   "Currency", "Country", "City", "ReputationScore",
                   "AverageRating", "TotalReviews",
                   "IsVerifiedSeller", "IsVerifiedBuyer",
                   "CreatedAt"
            FROM users
            WHERE LOWER("Email") = LOWER(@Email) AND "IsDeleted" = false
            """;
        
        return await connection.QuerySingleOrDefaultAsync<UserDto>(sql, new { Email = email });
    }

    public async Task<UserDto?> GetByUsernameAsync(string username)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();
        
        const string sql = """
            SELECT "Id", "Email", "Username", "FirstName", "LastName",
                   "PhoneNumber", "AvatarUrl", "Bio", "Status",
                   "EmailVerified", "PhoneVerified",
                   "PreferredLanguage", "TimeZone",
                   "Currency", "Country", "City", "ReputationScore",
                   "AverageRating", "TotalReviews",
                   "IsVerifiedSeller", "IsVerifiedBuyer",
                   "CreatedAt"
            FROM users
            WHERE LOWER("Username") = LOWER(@Username) AND "IsDeleted" = false
            """;
        
        return await connection.QuerySingleOrDefaultAsync<UserDto>(sql, new { Username = username });
    }

    public async Task<(IEnumerable<UserListDto> Users, int TotalCount)> GetAllAsync(int page, int pageSize, string? search = null, string? status = null)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();
        
        var whereClause = """WHERE "IsDeleted" = false""";
        if (!string.IsNullOrEmpty(search))
        {
            whereClause += """ AND (LOWER("Username") LIKE LOWER(@Search) OR LOWER("FirstName") LIKE LOWER(@Search) OR LOWER("LastName") LIKE LOWER(@Search) OR LOWER("Email") LIKE LOWER(@Search))""";
        }
        if (!string.IsNullOrEmpty(status))
        {
            whereClause += """ AND "Status" = @Status""";
        }

        var countSql = $"SELECT COUNT(*) FROM users {whereClause}";
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, new { Search = $"%{search}%", Status = status });

        var sql = $"""
            SELECT "Id", "Username", "FirstName", "LastName",
                   "AvatarUrl", "Status", "AverageRating",
                   "TotalReviews", "IsVerifiedSeller"
            FROM users
            {whereClause}
            ORDER BY "CreatedAt" DESC
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
            INSERT INTO users ("Id", "Email", "Username", "PasswordHash", "FirstName", "LastName", "PhoneNumber",
                              "Status", "EmailVerified", "PhoneVerified", "TwoFactorEnabled",
                              "FailedLoginAttempts", "PreferredLanguage",
                              "ReputationScore", "TotalReviews", "AverageRating",
                              "IsVerifiedSeller", "IsVerifiedBuyer",
                              "CreatedAt", "IsDeleted")
            VALUES (@Id, @Email, @Username, @PasswordHash, @FirstName, @LastName, @PhoneNumber,
                   1, false, false, false,
                   0, 'en',
                   0, 0, 0,
                   false, false,
                   NOW(), false)
            RETURNING "Id"
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

        if (dto.FirstName != null) { updates.Add("\"FirstName\" = @FirstName"); parameters.Add("FirstName", dto.FirstName); }
        if (dto.LastName != null) { updates.Add("\"LastName\" = @LastName"); parameters.Add("LastName", dto.LastName); }
        if (dto.PhoneNumber != null) { updates.Add("\"PhoneNumber\" = @PhoneNumber"); parameters.Add("PhoneNumber", dto.PhoneNumber); }
        if (dto.Bio != null) { updates.Add("\"Bio\" = @Bio"); parameters.Add("Bio", dto.Bio); }
        if (dto.AvatarUrl != null) { updates.Add("\"AvatarUrl\" = @AvatarUrl"); parameters.Add("AvatarUrl", dto.AvatarUrl); }
        if (dto.PreferredLanguage != null) { updates.Add("\"PreferredLanguage\" = @PreferredLanguage"); parameters.Add("PreferredLanguage", dto.PreferredLanguage); }
        if (dto.TimeZone != null) { updates.Add("\"TimeZone\" = @TimeZone"); parameters.Add("TimeZone", dto.TimeZone); }
        if (dto.Currency != null) { updates.Add("\"Currency\" = @Currency"); parameters.Add("Currency", dto.Currency); }
        if (dto.Country != null) { updates.Add("\"Country\" = @Country"); parameters.Add("Country", dto.Country); }
        if (dto.City != null) { updates.Add("\"City\" = @City"); parameters.Add("City", dto.City); }
        if (dto.Address != null) { updates.Add("\"Address\" = @Address"); parameters.Add("Address", dto.Address); }

        if (updates.Count == 0) return true;

        updates.Add("\"UpdatedAt\" = NOW()");

        var sql = $"""UPDATE users SET {string.Join(", ", updates)} WHERE "Id" = @Id AND "IsDeleted" = false""";
        var rowsAffected = await connection.ExecuteAsync(sql, parameters);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();
        
        const string sql = """UPDATE users SET "IsDeleted" = true, "DeletedAt" = NOW() WHERE "Id" = @Id""";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<bool> UpdateLastLoginAsync(Guid id, string ipAddress)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();
        
        const string sql = """
            UPDATE users
            SET "LastLoginAt" = NOW(), "LastLoginIp" = @IpAddress, "FailedLoginAttempts" = 0
            WHERE "Id" = @Id
            """;
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, IpAddress = ipAddress });
        return rowsAffected > 0;
    }

    public async Task<string?> GetPasswordHashAsync(Guid id)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();
        
        const string sql = """SELECT "PasswordHash" FROM users WHERE "Id" = @Id AND "IsDeleted" = false""";
        return await connection.ExecuteScalarAsync<string?>(sql, new { Id = id });
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();
        
        const string sql = """SELECT EXISTS(SELECT 1 FROM users WHERE LOWER("Email") = LOWER(@Email) AND "IsDeleted" = false)""";
        return await connection.ExecuteScalarAsync<bool>(sql, new { Email = email });
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();
        
        const string sql = """SELECT EXISTS(SELECT 1 FROM users WHERE LOWER("Username") = LOWER(@Username) AND "IsDeleted" = false)""";
        return await connection.ExecuteScalarAsync<bool>(sql, new { Username = username });
    }

    public async Task<UserProfileDto?> GetProfileAsync(Guid userId)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();
        
        const string sql = """
            SELECT "Id", "UserId", "CompanyName", "Website",
                   "LinkedInUrl", "GitHubUrl", "PortfolioUrl",
                   "Headline", "About", "YearsOfExperience", "HourlyRate",
                   "AvailableForHire", "CompletedProjects",
                   "TotalEarnings", "IdVerified"
            FROM user_profiles
            WHERE "UserId" = @UserId AND "IsDeleted" = false
            """;
        
        return await connection.QuerySingleOrDefaultAsync<UserProfileDto>(sql, new { UserId = userId });
    }

    public async Task<bool> UpdateProfileAsync(Guid userId, UpdateProfileDto dto)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();
        
        // Upsert profile
        const string sql = """
            INSERT INTO user_profiles ("Id", "UserId", "CompanyName", "Website", "LinkedInUrl", "GitHubUrl",
                                       "PortfolioUrl", "Headline", "About", "YearsOfExperience", "HourlyRate",
                                       "AvailableForHire", "CompletedProjects", "OngoingProjects", "TotalEarnings",
                                       "IdVerified", "CreatedAt", "IsDeleted")
            VALUES (@Id, @UserId, @CompanyName, @Website, @LinkedInUrl, @GitHubUrl, @PortfolioUrl,
                   @Headline, @About, @YearsOfExperience, @HourlyRate, @AvailableForHire, 0, 0, 0,
                   false, NOW(), false)
            ON CONFLICT ("UserId") 
            DO UPDATE SET
                "CompanyName" = COALESCE(@CompanyName, user_profiles."CompanyName"),
                "Website" = COALESCE(@Website, user_profiles."Website"),
                "LinkedInUrl" = COALESCE(@LinkedInUrl, user_profiles."LinkedInUrl"),
                "GitHubUrl" = COALESCE(@GitHubUrl, user_profiles."GitHubUrl"),
                "PortfolioUrl" = COALESCE(@PortfolioUrl, user_profiles."PortfolioUrl"),
                "Headline" = COALESCE(@Headline, user_profiles."Headline"),
                "About" = COALESCE(@About, user_profiles."About"),
                "YearsOfExperience" = COALESCE(@YearsOfExperience, user_profiles."YearsOfExperience"),
                "HourlyRate" = COALESCE(@HourlyRate, user_profiles."HourlyRate"),
                "AvailableForHire" = COALESCE(@AvailableForHire, user_profiles."AvailableForHire"),
                "UpdatedAt" = NOW()
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
            SELECT us."Id", us."UserId", us."SkillId", s."Name" as SkillName,
                   us."Level", us."YearsOfExperience",
                   us."VerificationStatus", us."TestScore",
                   us."IsEndorsed", us."EndorsementCount"
            FROM user_skills us
            JOIN skills s ON us."SkillId" = s."Id"
            WHERE us."UserId" = @UserId AND us."IsDeleted" = false
            ORDER BY us."IsPrimary" DESC, us."EndorsementCount" DESC
            """;
        
        return await connection.QueryAsync<UserSkillDto>(sql, new { UserId = userId });
    }

    public async Task<bool> AddUserSkillAsync(Guid userId, Guid skillId, string level, int yearsOfExperience)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();
        
        const string sql = """
            INSERT INTO user_skills ("Id", "UserId", "SkillId", "Level", "YearsOfExperience",
                                    "VerificationStatus", "IsPrimary", "IsEndorsed", "EndorsementCount",
                                    "CreatedAt", "IsDeleted")
            VALUES (@Id, @UserId, @SkillId, @Level, @YearsOfExperience,
                   0, false, false, 0,
                   NOW(), false)
            ON CONFLICT ("UserId", "SkillId") DO UPDATE SET
                "Level" = @Level,
                "YearsOfExperience" = @YearsOfExperience,
                "UpdatedAt" = NOW()
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
            SET "IsDeleted" = true, "DeletedAt" = NOW()
            WHERE "UserId" = @UserId AND "SkillId" = @SkillId
            """;
        
        var rowsAffected = await connection.ExecuteAsync(sql, new { UserId = userId, SkillId = skillId });
        return rowsAffected > 0;
    }
}
