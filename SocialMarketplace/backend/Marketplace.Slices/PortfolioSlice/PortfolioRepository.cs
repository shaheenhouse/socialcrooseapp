using Dapper;
using Marketplace.Core.Infrastructure;

namespace Marketplace.Slices.PortfolioSlice;

public interface IPortfolioRepository
{
    Task<PortfolioDto?> GetByIdAsync(Guid id);
    Task<PortfolioDto?> GetBySlugAsync(string slug);
    Task<PortfolioDto?> GetByUserIdAsync(Guid userId);
    Task<Guid> CreateAsync(CreatePortfolioDto dto, Guid userId);
    Task<bool> UpdateAsync(Guid id, UpdatePortfolioDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<PortfolioListDto>> GetPublicPortfoliosAsync(int page, int pageSize);
    Task<int> GetPublicCountAsync();
}

public class PortfolioRepository : IPortfolioRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public PortfolioRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<PortfolioDto?> GetByIdAsync(Guid id)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        const string sql = """
            SELECT p."Id", p."UserId", p."Slug", p."IsPublic", p."Theme",
                   p."PersonalInfo", p."Education", p."Experience",
                   p."Skills", p."Roles", p."Certifications",
                   p."Projects", p."Achievements", p."Languages",
                   p."Resumes", p."CreatedAt", p."UpdatedAt"
            FROM portfolios p
            WHERE p."Id" = @Id AND p."IsDeleted" = false
            """;

        return await connection.QuerySingleOrDefaultAsync<PortfolioDto>(sql, new { Id = id });
    }

    public async Task<PortfolioDto?> GetBySlugAsync(string slug)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        const string sql = """
            SELECT p."Id", p."UserId", p."Slug", p."IsPublic", p."Theme",
                   p."PersonalInfo", p."Education", p."Experience",
                   p."Skills", p."Roles", p."Certifications",
                   p."Projects", p."Achievements", p."Languages",
                   p."Resumes", p."CreatedAt", p."UpdatedAt"
            FROM portfolios p
            WHERE p."Slug" = @Slug AND p."IsDeleted" = false
            """;

        return await connection.QuerySingleOrDefaultAsync<PortfolioDto>(sql, new { Slug = slug });
    }

    public async Task<PortfolioDto?> GetByUserIdAsync(Guid userId)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        const string sql = """
            SELECT p."Id", p."UserId", p."Slug", p."IsPublic", p."Theme",
                   p."PersonalInfo", p."Education", p."Experience",
                   p."Skills", p."Roles", p."Certifications",
                   p."Projects", p."Achievements", p."Languages",
                   p."Resumes", p."CreatedAt", p."UpdatedAt"
            FROM portfolios p
            WHERE p."UserId" = @UserId AND p."IsDeleted" = false
            LIMIT 1
            """;

        return await connection.QuerySingleOrDefaultAsync<PortfolioDto>(sql, new { UserId = userId });
    }

    public async Task<Guid> CreateAsync(CreatePortfolioDto dto, Guid userId)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();

        var id = Guid.NewGuid();

        const string sql = """
            INSERT INTO portfolios ("Id", "UserId", "Slug", "IsPublic", "Theme",
                                   "PersonalInfo", "Education", "Experience", "Skills", "Roles",
                                   "Certifications", "Projects", "Achievements", "Languages", "Resumes",
                                   "CreatedAt", "IsDeleted")
            VALUES (@Id, @UserId, @Slug, @IsPublic, @Theme,
                   @PersonalInfo::jsonb, @Education::jsonb, @Experience::jsonb, @Skills::jsonb, @Roles::jsonb,
                   @Certifications::jsonb, @Projects::jsonb, @Achievements::jsonb, @Languages::jsonb, @Resumes::jsonb,
                   NOW(), false)
            RETURNING "Id"
            """;

        return await connection.ExecuteScalarAsync<Guid>(sql, new
        {
            Id = id,
            UserId = userId,
            dto.Slug,
            dto.IsPublic,
            dto.Theme,
            PersonalInfo = dto.PersonalInfo ?? "{}",
            Education = dto.Education ?? "[]",
            Experience = dto.Experience ?? "[]",
            Skills = dto.Skills ?? "[]",
            Roles = dto.Roles ?? "[]",
            Certifications = dto.Certifications ?? "[]",
            Projects = dto.Projects ?? "[]",
            Achievements = dto.Achievements ?? "[]",
            Languages = dto.Languages ?? "[]",
            Resumes = dto.Resumes ?? "[]"
        });
    }

    public async Task<bool> UpdateAsync(Guid id, UpdatePortfolioDto dto)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();

        var updates = new List<string>();
        var parameters = new DynamicParameters();
        parameters.Add("Id", id);

        if (dto.Slug != null) { updates.Add(@"""Slug"" = @Slug"); parameters.Add("Slug", dto.Slug); }
        if (dto.IsPublic.HasValue) { updates.Add(@"""IsPublic"" = @IsPublic"); parameters.Add("IsPublic", dto.IsPublic.Value); }
        if (dto.Theme != null) { updates.Add(@"""Theme"" = @Theme"); parameters.Add("Theme", dto.Theme); }
        if (dto.PersonalInfo != null) { updates.Add(@"""PersonalInfo"" = @PersonalInfo::jsonb"); parameters.Add("PersonalInfo", dto.PersonalInfo); }
        if (dto.Education != null) { updates.Add(@"""Education"" = @Education::jsonb"); parameters.Add("Education", dto.Education); }
        if (dto.Experience != null) { updates.Add(@"""Experience"" = @Experience::jsonb"); parameters.Add("Experience", dto.Experience); }
        if (dto.Skills != null) { updates.Add(@"""Skills"" = @Skills::jsonb"); parameters.Add("Skills", dto.Skills); }
        if (dto.Roles != null) { updates.Add(@"""Roles"" = @Roles::jsonb"); parameters.Add("Roles", dto.Roles); }
        if (dto.Certifications != null) { updates.Add(@"""Certifications"" = @Certifications::jsonb"); parameters.Add("Certifications", dto.Certifications); }
        if (dto.Projects != null) { updates.Add(@"""Projects"" = @Projects::jsonb"); parameters.Add("Projects", dto.Projects); }
        if (dto.Achievements != null) { updates.Add(@"""Achievements"" = @Achievements::jsonb"); parameters.Add("Achievements", dto.Achievements); }
        if (dto.Languages != null) { updates.Add(@"""Languages"" = @Languages::jsonb"); parameters.Add("Languages", dto.Languages); }
        if (dto.Resumes != null) { updates.Add(@"""Resumes"" = @Resumes::jsonb"); parameters.Add("Resumes", dto.Resumes); }

        if (updates.Count == 0) return true;
        updates.Add(@"""UpdatedAt"" = NOW()");

        var sql = $@"UPDATE portfolios SET {string.Join(", ", updates)} WHERE ""Id"" = @Id AND ""IsDeleted"" = false";
        return await connection.ExecuteAsync(sql, parameters) > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();
        return await connection.ExecuteAsync(
            @"UPDATE portfolios SET ""IsDeleted"" = true, ""DeletedAt"" = NOW() WHERE ""Id"" = @Id", new { Id = id }) > 0;
    }

    public async Task<IEnumerable<PortfolioListDto>> GetPublicPortfoliosAsync(int page, int pageSize)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        const string sql = """
            SELECT p."Id", p."Slug", p."Theme", p."PersonalInfo", p."CreatedAt",
                   u."FirstName" || ' ' || u."LastName" as OwnerName, u."AvatarUrl" as OwnerAvatarUrl
            FROM portfolios p
            JOIN users u ON p."UserId" = u."Id"
            WHERE p."IsPublic" = true AND p."IsDeleted" = false
            ORDER BY p."UpdatedAt" DESC
            LIMIT @PageSize OFFSET @Offset
            """;

        return await connection.QueryAsync<PortfolioListDto>(sql, new
        {
            PageSize = pageSize,
            Offset = (page - 1) * pageSize
        });
    }

    public async Task<int> GetPublicCountAsync()
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(
            @"SELECT COUNT(*) FROM portfolios WHERE ""IsPublic"" = true AND ""IsDeleted"" = false");
    }
}

// DTOs
public record PortfolioDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Slug { get; init; } = string.Empty;
    public bool IsPublic { get; init; }
    public string Theme { get; init; } = "dark";
    public string PersonalInfo { get; init; } = "{}";
    public string Education { get; init; } = "[]";
    public string Experience { get; init; } = "[]";
    public string Skills { get; init; } = "[]";
    public string Roles { get; init; } = "[]";
    public string Certifications { get; init; } = "[]";
    public string Projects { get; init; } = "[]";
    public string Achievements { get; init; } = "[]";
    public string Languages { get; init; } = "[]";
    public string Resumes { get; init; } = "[]";
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public record PortfolioListDto
{
    public Guid Id { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string Theme { get; init; } = "dark";
    public string PersonalInfo { get; init; } = "{}";
    public DateTime CreatedAt { get; init; }
    public string? OwnerName { get; init; }
    public string? OwnerAvatarUrl { get; init; }
}

public record CreatePortfolioDto(
    string Slug,
    bool IsPublic = false,
    string Theme = "dark",
    string? PersonalInfo = null,
    string? Education = null,
    string? Experience = null,
    string? Skills = null,
    string? Roles = null,
    string? Certifications = null,
    string? Projects = null,
    string? Achievements = null,
    string? Languages = null,
    string? Resumes = null);

public record UpdatePortfolioDto(
    string? Slug = null,
    bool? IsPublic = null,
    string? Theme = null,
    string? PersonalInfo = null,
    string? Education = null,
    string? Experience = null,
    string? Skills = null,
    string? Roles = null,
    string? Certifications = null,
    string? Projects = null,
    string? Achievements = null,
    string? Languages = null,
    string? Resumes = null);
