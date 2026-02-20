using Dapper;
using Marketplace.Core.Infrastructure;

namespace Marketplace.Slices.DesignSlice;

public interface IDesignRepository
{
    Task<DesignDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<DesignListDto>> GetByUserIdAsync(Guid userId, int page, int pageSize);
    Task<int> GetCountByUserIdAsync(Guid userId);
    Task<IEnumerable<DesignListDto>> GetTemplatesAsync(int page, int pageSize, string? category = null);
    Task<int> GetTemplateCountAsync(string? category = null);
    Task<Guid> CreateAsync(CreateDesignDto dto, Guid userId);
    Task<bool> UpdateAsync(Guid id, UpdateDesignDto dto);
    Task<bool> DeleteAsync(Guid id);
}

public class DesignRepository : IDesignRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public DesignRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<DesignDto?> GetByIdAsync(Guid id)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        const string sql = """
            SELECT d.id, d.user_id as UserId, d.name, d.description, d.width, d.height,
                   d.canvas_json as CanvasJson, d.thumbnail, d.status, d.category,
                   d.tags as Tags, d.is_template as IsTemplate, d.is_public as IsPublic,
                   d.created_at as CreatedAt, d.updated_at as UpdatedAt
            FROM designs d
            WHERE d.id = @Id AND d.is_deleted = false
            """;

        return await connection.QuerySingleOrDefaultAsync<DesignDto>(sql, new { Id = id });
    }

    public async Task<IEnumerable<DesignListDto>> GetByUserIdAsync(Guid userId, int page, int pageSize)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        const string sql = """
            SELECT d.id, d.name, d.description, d.width, d.height, d.thumbnail,
                   d.status, d.category, d.is_template as IsTemplate, d.is_public as IsPublic,
                   d.created_at as CreatedAt, d.updated_at as UpdatedAt
            FROM designs d
            WHERE d.user_id = @UserId AND d.is_deleted = false
            ORDER BY d.updated_at DESC NULLS LAST, d.created_at DESC
            LIMIT @PageSize OFFSET @Offset
            """;

        return await connection.QueryAsync<DesignListDto>(sql, new
        {
            UserId = userId,
            PageSize = pageSize,
            Offset = (page - 1) * pageSize
        });
    }

    public async Task<int> GetCountByUserIdAsync(Guid userId)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();
        return await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM designs WHERE user_id = @UserId AND is_deleted = false",
            new { UserId = userId });
    }

    public async Task<IEnumerable<DesignListDto>> GetTemplatesAsync(int page, int pageSize, string? category = null)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        var whereClause = "WHERE d.is_template = true AND d.is_deleted = false";
        if (!string.IsNullOrEmpty(category))
            whereClause += " AND d.category = @Category";

        var sql = $"""
            SELECT d.id, d.name, d.description, d.width, d.height, d.thumbnail,
                   d.status, d.category, d.is_template as IsTemplate, d.is_public as IsPublic,
                   d.created_at as CreatedAt, d.updated_at as UpdatedAt
            FROM designs d
            {whereClause}
            ORDER BY d.created_at DESC
            LIMIT @PageSize OFFSET @Offset
            """;

        return await connection.QueryAsync<DesignListDto>(sql, new
        {
            Category = category,
            PageSize = pageSize,
            Offset = (page - 1) * pageSize
        });
    }

    public async Task<int> GetTemplateCountAsync(string? category = null)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        var whereClause = "WHERE is_template = true AND is_deleted = false";
        if (!string.IsNullOrEmpty(category))
            whereClause += " AND category = @Category";

        return await connection.ExecuteScalarAsync<int>(
            $"SELECT COUNT(*) FROM designs {whereClause}", new { Category = category });
    }

    public async Task<Guid> CreateAsync(CreateDesignDto dto, Guid userId)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();

        var id = Guid.NewGuid();

        const string sql = """
            INSERT INTO designs (id, user_id, name, description, width, height,
                                canvas_json, thumbnail, status, category, tags,
                                is_template, is_public, created_at, is_deleted)
            VALUES (@Id, @UserId, @Name, @Description, @Width, @Height,
                   @CanvasJson, @Thumbnail, @Status, @Category, @Tags::jsonb,
                   @IsTemplate, @IsPublic, NOW(), false)
            RETURNING id
            """;

        return await connection.ExecuteScalarAsync<Guid>(sql, new
        {
            Id = id,
            UserId = userId,
            dto.Name,
            dto.Description,
            dto.Width,
            dto.Height,
            CanvasJson = dto.CanvasJson ?? "{}",
            Thumbnail = dto.Thumbnail ?? "",
            Status = dto.Status ?? "Draft",
            Category = dto.Category ?? "custom",
            Tags = dto.Tags ?? "[]",
            dto.IsTemplate,
            dto.IsPublic
        });
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateDesignDto dto)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();

        var updates = new List<string>();
        var parameters = new DynamicParameters();
        parameters.Add("Id", id);

        if (dto.Name != null) { updates.Add("name = @Name"); parameters.Add("Name", dto.Name); }
        if (dto.Description != null) { updates.Add("description = @Description"); parameters.Add("Description", dto.Description); }
        if (dto.Width.HasValue) { updates.Add("width = @Width"); parameters.Add("Width", dto.Width.Value); }
        if (dto.Height.HasValue) { updates.Add("height = @Height"); parameters.Add("Height", dto.Height.Value); }
        if (dto.CanvasJson != null) { updates.Add("canvas_json = @CanvasJson"); parameters.Add("CanvasJson", dto.CanvasJson); }
        if (dto.Thumbnail != null) { updates.Add("thumbnail = @Thumbnail"); parameters.Add("Thumbnail", dto.Thumbnail); }
        if (dto.Status != null) { updates.Add("status = @Status"); parameters.Add("Status", dto.Status); }
        if (dto.Category != null) { updates.Add("category = @Category"); parameters.Add("Category", dto.Category); }
        if (dto.Tags != null) { updates.Add("tags = @Tags::jsonb"); parameters.Add("Tags", dto.Tags); }
        if (dto.IsTemplate.HasValue) { updates.Add("is_template = @IsTemplate"); parameters.Add("IsTemplate", dto.IsTemplate.Value); }
        if (dto.IsPublic.HasValue) { updates.Add("is_public = @IsPublic"); parameters.Add("IsPublic", dto.IsPublic.Value); }

        if (updates.Count == 0) return true;
        updates.Add("updated_at = NOW()");

        var sql = $"UPDATE designs SET {string.Join(", ", updates)} WHERE id = @Id AND is_deleted = false";
        return await connection.ExecuteAsync(sql, parameters) > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();
        return await connection.ExecuteAsync(
            "UPDATE designs SET is_deleted = true, deleted_at = NOW() WHERE id = @Id", new { Id = id }) > 0;
    }
}

// DTOs
public record DesignDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Name { get; init; } = "Untitled Design";
    public string? Description { get; init; }
    public int Width { get; init; } = 1080;
    public int Height { get; init; } = 1080;
    public string CanvasJson { get; init; } = "{}";
    public string Thumbnail { get; init; } = string.Empty;
    public string Status { get; init; } = "Draft";
    public string Category { get; init; } = "custom";
    public string? Tags { get; init; }
    public bool IsTemplate { get; init; }
    public bool IsPublic { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public record DesignListDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = "Untitled Design";
    public string? Description { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public string Thumbnail { get; init; } = string.Empty;
    public string Status { get; init; } = "Draft";
    public string Category { get; init; } = "custom";
    public bool IsTemplate { get; init; }
    public bool IsPublic { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public record CreateDesignDto(
    string Name = "Untitled Design",
    string? Description = null,
    int Width = 1080,
    int Height = 1080,
    string? CanvasJson = null,
    string? Thumbnail = null,
    string? Status = null,
    string? Category = null,
    string? Tags = null,
    bool IsTemplate = false,
    bool IsPublic = false);

public record UpdateDesignDto(
    string? Name = null,
    string? Description = null,
    int? Width = null,
    int? Height = null,
    string? CanvasJson = null,
    string? Thumbnail = null,
    string? Status = null,
    string? Category = null,
    string? Tags = null,
    bool? IsTemplate = null,
    bool? IsPublic = null);
