using Dapper;
using Marketplace.Core.Infrastructure;

namespace Marketplace.Slices.ResumeSlice;

public interface IResumeRepository
{
    Task<ResumeDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ResumeListDto>> GetByUserIdAsync(Guid userId);
    Task<Guid> CreateAsync(CreateResumeDto dto, Guid userId);
    Task<bool> UpdateAsync(Guid id, UpdateResumeDto dto);
    Task<bool> DeleteAsync(Guid id);
}

public class ResumeRepository : IResumeRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public ResumeRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<ResumeDto?> GetByIdAsync(Guid id)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        const string sql = """
            SELECT r."Id", r."UserId", r."Title", r."Template", r."IsPublic", r."PdfUrl",
                   r."PersonalInfo", r."Education", r."Experience",
                   r."Skills", r."Certifications", r."Projects",
                   r."Languages", r."CustomSections",
                   r."CreatedAt", r."UpdatedAt"
            FROM resumes r
            WHERE r."Id" = @Id AND r."IsDeleted" = false
            """;

        return await connection.QuerySingleOrDefaultAsync<ResumeDto>(sql, new { Id = id });
    }

    public async Task<IEnumerable<ResumeListDto>> GetByUserIdAsync(Guid userId)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        const string sql = """
            SELECT r."Id", r."Title", r."Template", r."IsPublic", r."PdfUrl",
                   r."CreatedAt", r."UpdatedAt"
            FROM resumes r
            WHERE r."UserId" = @UserId AND r."IsDeleted" = false
            ORDER BY r."UpdatedAt" DESC NULLS LAST, r."CreatedAt" DESC
            """;

        return await connection.QueryAsync<ResumeListDto>(sql, new { UserId = userId });
    }

    public async Task<Guid> CreateAsync(CreateResumeDto dto, Guid userId)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();

        var id = Guid.NewGuid();

        const string sql = """
            INSERT INTO resumes ("Id", "UserId", "Title", "Template", "IsPublic",
                                "PersonalInfo", "Education", "Experience", "Skills",
                                "Certifications", "Projects", "Languages", "CustomSections",
                                "CreatedAt", "IsDeleted")
            VALUES (@Id, @UserId, @Title, @Template, @IsPublic,
                   @PersonalInfo::jsonb, @Education::jsonb, @Experience::jsonb, @Skills::jsonb,
                   @Certifications::jsonb, @Projects::jsonb, @Languages::jsonb, @CustomSections::jsonb,
                   NOW(), false)
            RETURNING "Id"
            """;

        return await connection.ExecuteScalarAsync<Guid>(sql, new
        {
            Id = id,
            UserId = userId,
            dto.Title,
            dto.Template,
            dto.IsPublic,
            PersonalInfo = dto.PersonalInfo ?? "{}",
            Education = dto.Education ?? "[]",
            Experience = dto.Experience ?? "[]",
            Skills = dto.Skills ?? "[]",
            Certifications = dto.Certifications ?? "[]",
            Projects = dto.Projects ?? "[]",
            Languages = dto.Languages ?? "[]",
            CustomSections = dto.CustomSections ?? "[]"
        });
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateResumeDto dto)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();

        var updates = new List<string>();
        var parameters = new DynamicParameters();
        parameters.Add("Id", id);

        if (dto.Title != null) { updates.Add(@"""Title"" = @Title"); parameters.Add("Title", dto.Title); }
        if (dto.Template != null) { updates.Add(@"""Template"" = @Template"); parameters.Add("Template", dto.Template); }
        if (dto.IsPublic.HasValue) { updates.Add(@"""IsPublic"" = @IsPublic"); parameters.Add("IsPublic", dto.IsPublic.Value); }
        if (dto.PdfUrl != null) { updates.Add(@"""PdfUrl"" = @PdfUrl"); parameters.Add("PdfUrl", dto.PdfUrl); }
        if (dto.PersonalInfo != null) { updates.Add(@"""PersonalInfo"" = @PersonalInfo::jsonb"); parameters.Add("PersonalInfo", dto.PersonalInfo); }
        if (dto.Education != null) { updates.Add(@"""Education"" = @Education::jsonb"); parameters.Add("Education", dto.Education); }
        if (dto.Experience != null) { updates.Add(@"""Experience"" = @Experience::jsonb"); parameters.Add("Experience", dto.Experience); }
        if (dto.Skills != null) { updates.Add(@"""Skills"" = @Skills::jsonb"); parameters.Add("Skills", dto.Skills); }
        if (dto.Certifications != null) { updates.Add(@"""Certifications"" = @Certifications::jsonb"); parameters.Add("Certifications", dto.Certifications); }
        if (dto.Projects != null) { updates.Add(@"""Projects"" = @Projects::jsonb"); parameters.Add("Projects", dto.Projects); }
        if (dto.Languages != null) { updates.Add(@"""Languages"" = @Languages::jsonb"); parameters.Add("Languages", dto.Languages); }
        if (dto.CustomSections != null) { updates.Add(@"""CustomSections"" = @CustomSections::jsonb"); parameters.Add("CustomSections", dto.CustomSections); }

        if (updates.Count == 0) return true;
        updates.Add(@"""UpdatedAt"" = NOW()");

        var sql = $@"UPDATE resumes SET {string.Join(", ", updates)} WHERE ""Id"" = @Id AND ""IsDeleted"" = false";
        return await connection.ExecuteAsync(sql, parameters) > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();
        return await connection.ExecuteAsync(
            @"UPDATE resumes SET ""IsDeleted"" = true, ""DeletedAt"" = NOW() WHERE ""Id"" = @Id", new { Id = id }) > 0;
    }
}

// DTOs
public record ResumeDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Title { get; init; } = "My Resume";
    public string Template { get; init; } = "modern";
    public bool IsPublic { get; init; }
    public string? PdfUrl { get; init; }
    public string PersonalInfo { get; init; } = "{}";
    public string Education { get; init; } = "[]";
    public string Experience { get; init; } = "[]";
    public string Skills { get; init; } = "[]";
    public string Certifications { get; init; } = "[]";
    public string Projects { get; init; } = "[]";
    public string Languages { get; init; } = "[]";
    public string CustomSections { get; init; } = "[]";
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public record ResumeListDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = "My Resume";
    public string Template { get; init; } = "modern";
    public bool IsPublic { get; init; }
    public string? PdfUrl { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public record CreateResumeDto(
    string Title = "My Resume",
    string Template = "modern",
    bool IsPublic = false,
    string? PersonalInfo = null,
    string? Education = null,
    string? Experience = null,
    string? Skills = null,
    string? Certifications = null,
    string? Projects = null,
    string? Languages = null,
    string? CustomSections = null);

public record UpdateResumeDto(
    string? Title = null,
    string? Template = null,
    bool? IsPublic = null,
    string? PdfUrl = null,
    string? PersonalInfo = null,
    string? Education = null,
    string? Experience = null,
    string? Skills = null,
    string? Certifications = null,
    string? Projects = null,
    string? Languages = null,
    string? CustomSections = null);
