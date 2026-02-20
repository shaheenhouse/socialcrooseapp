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
            SELECT d."Id", d."UserId", d."Name", d."Description", d."Width", d."Height",
                   d."CanvasJson", d."Thumbnail", d."Status", d."Category",
                   d."Tags", d."IsTemplate", d."IsPublic",
                   d."CreatedAt", d."UpdatedAt"
            FROM designs d
            WHERE d."Id" = @Id AND d."IsDeleted" = false
            """;

        return await connection.QuerySingleOrDefaultAsync<DesignDto>(sql, new { Id = id });
    }

    public async Task<IEnumerable<DesignListDto>> GetByUserIdAsync(Guid userId, int page, int pageSize)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        const string sql = """
            SELECT d."Id", d."Name", d."Description", d."Width", d."Height", d."Thumbnail",
                   d."Status", d."Category", d."IsTemplate", d."IsPublic",
                   d."CreatedAt", d."UpdatedAt"
            FROM designs d
            WHERE d."UserId" = @UserId AND d."IsDeleted" = false
            ORDER BY d."UpdatedAt" DESC NULLS LAST, d."CreatedAt" DESC
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
            @"SELECT COUNT(*) FROM designs WHERE ""UserId"" = @UserId AND ""IsDeleted"" = false",
            new { UserId = userId });
    }

    public async Task<IEnumerable<DesignListDto>> GetTemplatesAsync(int page, int pageSize, string? category = null)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        var whereClause = @"WHERE d.""IsTemplate"" = true AND d.""IsDeleted"" = false";
        if (!string.IsNullOrEmpty(category))
            whereClause += @" AND d.""Category"" = @Category";

        var sql = $"""
            SELECT d."Id", d."Name", d."Description", d."Width", d."Height", d."Thumbnail",
                   d."Status", d."Category", d."IsTemplate", d."IsPublic",
                   d."CreatedAt", d."UpdatedAt"
            FROM designs d
            {whereClause}
            ORDER BY d."CreatedAt" DESC
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

        var whereClause = @"WHERE ""IsTemplate"" = true AND ""IsDeleted"" = false";
        if (!string.IsNullOrEmpty(category))
            whereClause += @" AND ""Category"" = @Category";

        return await connection.ExecuteScalarAsync<int>(
            $"SELECT COUNT(*) FROM designs {whereClause}", new { Category = category });
    }

    public async Task<Guid> CreateAsync(CreateDesignDto dto, Guid userId)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();

        var id = Guid.NewGuid();

        const string sql = """
            INSERT INTO designs ("Id", "UserId", "Name", "Description", "Width", "Height",
                                "CanvasJson", "Thumbnail", "Status", "Category", "Tags",
                                "IsTemplate", "IsPublic", "CreatedAt", "IsDeleted")
            VALUES (@Id, @UserId, @Name, @Description, @Width, @Height,
                   @CanvasJson, @Thumbnail, @Status, @Category, @Tags::jsonb,
                   @IsTemplate, @IsPublic, NOW(), false)
            RETURNING "Id"
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

        if (dto.Name != null) { updates.Add(@"""Name"" = @Name"); parameters.Add("Name", dto.Name); }
        if (dto.Description != null) { updates.Add(@"""Description"" = @Description"); parameters.Add("Description", dto.Description); }
        if (dto.Width.HasValue) { updates.Add(@"""Width"" = @Width"); parameters.Add("Width", dto.Width.Value); }
        if (dto.Height.HasValue) { updates.Add(@"""Height"" = @Height"); parameters.Add("Height", dto.Height.Value); }
        if (dto.CanvasJson != null) { updates.Add(@"""CanvasJson"" = @CanvasJson"); parameters.Add("CanvasJson", dto.CanvasJson); }
        if (dto.Thumbnail != null) { updates.Add(@"""Thumbnail"" = @Thumbnail"); parameters.Add("Thumbnail", dto.Thumbnail); }
        if (dto.Status != null) { updates.Add(@"""Status"" = @Status"); parameters.Add("Status", dto.Status); }
        if (dto.Category != null) { updates.Add(@"""Category"" = @Category"); parameters.Add("Category", dto.Category); }
        if (dto.Tags != null) { updates.Add(@"""Tags"" = @Tags::jsonb"); parameters.Add("Tags", dto.Tags); }
        if (dto.IsTemplate.HasValue) { updates.Add(@"""IsTemplate"" = @IsTemplate"); parameters.Add("IsTemplate", dto.IsTemplate.Value); }
        if (dto.IsPublic.HasValue) { updates.Add(@"""IsPublic"" = @IsPublic"); parameters.Add("IsPublic", dto.IsPublic.Value); }

        if (updates.Count == 0) return true;
        updates.Add(@"""UpdatedAt"" = NOW()");

        var sql = $@"UPDATE designs SET {string.Join(", ", updates)} WHERE ""Id"" = @Id AND ""IsDeleted"" = false";
        return await connection.ExecuteAsync(sql, parameters) > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();
        return await connection.ExecuteAsync(
            @"UPDATE designs SET ""IsDeleted"" = true, ""DeletedAt"" = NOW() WHERE ""Id"" = @Id", new { Id = id }) > 0;
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
