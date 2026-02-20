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
            SELECT r.id, r.user_id as UserId, r.title, r.template, r.is_public as IsPublic, r.pdf_url as PdfUrl,
                   r.personal_info as PersonalInfo, r.education as Education, r.experience as Experience,
                   r.skills as Skills, r.certifications as Certifications, r.projects as Projects,
                   r.languages as Languages, r.custom_sections as CustomSections,
                   r.created_at as CreatedAt, r.updated_at as UpdatedAt
            FROM resumes r
            WHERE r.id = @Id AND r.is_deleted = false
            """;

        return await connection.QuerySingleOrDefaultAsync<ResumeDto>(sql, new { Id = id });
    }

    public async Task<IEnumerable<ResumeListDto>> GetByUserIdAsync(Guid userId)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        const string sql = """
            SELECT r.id, r.title, r.template, r.is_public as IsPublic, r.pdf_url as PdfUrl,
                   r.created_at as CreatedAt, r.updated_at as UpdatedAt
            FROM resumes r
            WHERE r.user_id = @UserId AND r.is_deleted = false
            ORDER BY r.updated_at DESC NULLS LAST, r.created_at DESC
            """;

        return await connection.QueryAsync<ResumeListDto>(sql, new { UserId = userId });
    }

    public async Task<Guid> CreateAsync(CreateResumeDto dto, Guid userId)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();

        var id = Guid.NewGuid();

        const string sql = """
            INSERT INTO resumes (id, user_id, title, template, is_public,
                                personal_info, education, experience, skills,
                                certifications, projects, languages, custom_sections,
                                created_at, is_deleted)
            VALUES (@Id, @UserId, @Title, @Template, @IsPublic,
                   @PersonalInfo::jsonb, @Education::jsonb, @Experience::jsonb, @Skills::jsonb,
                   @Certifications::jsonb, @Projects::jsonb, @Languages::jsonb, @CustomSections::jsonb,
                   NOW(), false)
            RETURNING id
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

        if (dto.Title != null) { updates.Add("title = @Title"); parameters.Add("Title", dto.Title); }
        if (dto.Template != null) { updates.Add("template = @Template"); parameters.Add("Template", dto.Template); }
        if (dto.IsPublic.HasValue) { updates.Add("is_public = @IsPublic"); parameters.Add("IsPublic", dto.IsPublic.Value); }
        if (dto.PdfUrl != null) { updates.Add("pdf_url = @PdfUrl"); parameters.Add("PdfUrl", dto.PdfUrl); }
        if (dto.PersonalInfo != null) { updates.Add("personal_info = @PersonalInfo::jsonb"); parameters.Add("PersonalInfo", dto.PersonalInfo); }
        if (dto.Education != null) { updates.Add("education = @Education::jsonb"); parameters.Add("Education", dto.Education); }
        if (dto.Experience != null) { updates.Add("experience = @Experience::jsonb"); parameters.Add("Experience", dto.Experience); }
        if (dto.Skills != null) { updates.Add("skills = @Skills::jsonb"); parameters.Add("Skills", dto.Skills); }
        if (dto.Certifications != null) { updates.Add("certifications = @Certifications::jsonb"); parameters.Add("Certifications", dto.Certifications); }
        if (dto.Projects != null) { updates.Add("projects = @Projects::jsonb"); parameters.Add("Projects", dto.Projects); }
        if (dto.Languages != null) { updates.Add("languages = @Languages::jsonb"); parameters.Add("Languages", dto.Languages); }
        if (dto.CustomSections != null) { updates.Add("custom_sections = @CustomSections::jsonb"); parameters.Add("CustomSections", dto.CustomSections); }

        if (updates.Count == 0) return true;
        updates.Add("updated_at = NOW()");

        var sql = $"UPDATE resumes SET {string.Join(", ", updates)} WHERE id = @Id AND is_deleted = false";
        return await connection.ExecuteAsync(sql, parameters) > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();
        return await connection.ExecuteAsync(
            "UPDATE resumes SET is_deleted = true, deleted_at = NOW() WHERE id = @Id", new { Id = id }) > 0;
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
