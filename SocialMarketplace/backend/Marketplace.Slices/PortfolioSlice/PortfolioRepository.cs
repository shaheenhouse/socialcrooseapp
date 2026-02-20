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
            SELECT p.id, p.user_id as UserId, p.slug, p.is_public as IsPublic, p.theme,
                   p.personal_info as PersonalInfo, p.education as Education, p.experience as Experience,
                   p.skills as Skills, p.roles as Roles, p.certifications as Certifications,
                   p.projects as Projects, p.achievements as Achievements, p.languages as Languages,
                   p.resumes as Resumes, p.created_at as CreatedAt, p.updated_at as UpdatedAt
            FROM portfolios p
            WHERE p.id = @Id AND p.is_deleted = false
            """;

        return await connection.QuerySingleOrDefaultAsync<PortfolioDto>(sql, new { Id = id });
    }

    public async Task<PortfolioDto?> GetBySlugAsync(string slug)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        const string sql = """
            SELECT p.id, p.user_id as UserId, p.slug, p.is_public as IsPublic, p.theme,
                   p.personal_info as PersonalInfo, p.education as Education, p.experience as Experience,
                   p.skills as Skills, p.roles as Roles, p.certifications as Certifications,
                   p.projects as Projects, p.achievements as Achievements, p.languages as Languages,
                   p.resumes as Resumes, p.created_at as CreatedAt, p.updated_at as UpdatedAt
            FROM portfolios p
            WHERE p.slug = @Slug AND p.is_deleted = false
            """;

        return await connection.QuerySingleOrDefaultAsync<PortfolioDto>(sql, new { Slug = slug });
    }

    public async Task<PortfolioDto?> GetByUserIdAsync(Guid userId)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        const string sql = """
            SELECT p.id, p.user_id as UserId, p.slug, p.is_public as IsPublic, p.theme,
                   p.personal_info as PersonalInfo, p.education as Education, p.experience as Experience,
                   p.skills as Skills, p.roles as Roles, p.certifications as Certifications,
                   p.projects as Projects, p.achievements as Achievements, p.languages as Languages,
                   p.resumes as Resumes, p.created_at as CreatedAt, p.updated_at as UpdatedAt
            FROM portfolios p
            WHERE p.user_id = @UserId AND p.is_deleted = false
            LIMIT 1
            """;

        return await connection.QuerySingleOrDefaultAsync<PortfolioDto>(sql, new { UserId = userId });
    }

    public async Task<Guid> CreateAsync(CreatePortfolioDto dto, Guid userId)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();

        var id = Guid.NewGuid();

        const string sql = """
            INSERT INTO portfolios (id, user_id, slug, is_public, theme,
                                   personal_info, education, experience, skills, roles,
                                   certifications, projects, achievements, languages, resumes,
                                   created_at, is_deleted)
            VALUES (@Id, @UserId, @Slug, @IsPublic, @Theme,
                   @PersonalInfo::jsonb, @Education::jsonb, @Experience::jsonb, @Skills::jsonb, @Roles::jsonb,
                   @Certifications::jsonb, @Projects::jsonb, @Achievements::jsonb, @Languages::jsonb, @Resumes::jsonb,
                   NOW(), false)
            RETURNING id
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

        if (dto.Slug != null) { updates.Add("slug = @Slug"); parameters.Add("Slug", dto.Slug); }
        if (dto.IsPublic.HasValue) { updates.Add("is_public = @IsPublic"); parameters.Add("IsPublic", dto.IsPublic.Value); }
        if (dto.Theme != null) { updates.Add("theme = @Theme"); parameters.Add("Theme", dto.Theme); }
        if (dto.PersonalInfo != null) { updates.Add("personal_info = @PersonalInfo::jsonb"); parameters.Add("PersonalInfo", dto.PersonalInfo); }
        if (dto.Education != null) { updates.Add("education = @Education::jsonb"); parameters.Add("Education", dto.Education); }
        if (dto.Experience != null) { updates.Add("experience = @Experience::jsonb"); parameters.Add("Experience", dto.Experience); }
        if (dto.Skills != null) { updates.Add("skills = @Skills::jsonb"); parameters.Add("Skills", dto.Skills); }
        if (dto.Roles != null) { updates.Add("roles = @Roles::jsonb"); parameters.Add("Roles", dto.Roles); }
        if (dto.Certifications != null) { updates.Add("certifications = @Certifications::jsonb"); parameters.Add("Certifications", dto.Certifications); }
        if (dto.Projects != null) { updates.Add("projects = @Projects::jsonb"); parameters.Add("Projects", dto.Projects); }
        if (dto.Achievements != null) { updates.Add("achievements = @Achievements::jsonb"); parameters.Add("Achievements", dto.Achievements); }
        if (dto.Languages != null) { updates.Add("languages = @Languages::jsonb"); parameters.Add("Languages", dto.Languages); }
        if (dto.Resumes != null) { updates.Add("resumes = @Resumes::jsonb"); parameters.Add("Resumes", dto.Resumes); }

        if (updates.Count == 0) return true;
        updates.Add("updated_at = NOW()");

        var sql = $"UPDATE portfolios SET {string.Join(", ", updates)} WHERE id = @Id AND is_deleted = false";
        return await connection.ExecuteAsync(sql, parameters) > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();
        return await connection.ExecuteAsync(
            "UPDATE portfolios SET is_deleted = true, deleted_at = NOW() WHERE id = @Id", new { Id = id }) > 0;
    }

    public async Task<IEnumerable<PortfolioListDto>> GetPublicPortfoliosAsync(int page, int pageSize)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        const string sql = """
            SELECT p.id, p.slug, p.theme, p.personal_info as PersonalInfo, p.created_at as CreatedAt,
                   u.first_name || ' ' || u.last_name as OwnerName, u.avatar_url as OwnerAvatarUrl
            FROM portfolios p
            JOIN users u ON p.user_id = u.id
            WHERE p.is_public = true AND p.is_deleted = false
            ORDER BY p.updated_at DESC
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
            "SELECT COUNT(*) FROM portfolios WHERE is_public = true AND is_deleted = false");
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
