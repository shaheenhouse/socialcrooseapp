using Dapper;
using Marketplace.Core.Infrastructure;
using Marketplace.Database.Enums;

namespace Marketplace.Slices.ProjectSlice;

public interface IProjectRepository
{
    Task<ProjectDto?> GetByIdAsync(Guid id);
    Task<(IEnumerable<ProjectListDto> Projects, int TotalCount)> GetAllAsync(ProjectQueryParams query);
    Task<(IEnumerable<ProjectListDto> Projects, int TotalCount)> GetByClientAsync(Guid clientId, int page, int pageSize);
    Task<(IEnumerable<ProjectListDto> Projects, int TotalCount)> GetByFreelancerAsync(Guid freelancerId, int page, int pageSize);
    Task<Guid> CreateAsync(CreateProjectDto dto, Guid clientId);
    Task<bool> UpdateAsync(Guid id, UpdateProjectDto dto);
    Task<IEnumerable<ProjectBidDto>> GetBidsAsync(Guid projectId, int page, int pageSize);
    Task<Guid> SubmitBidAsync(CreateBidDto dto, Guid freelancerId);
    Task<IEnumerable<ProjectMilestoneDto>> GetMilestonesAsync(Guid projectId);
    Task<Guid> CreateMilestoneAsync(CreateMilestoneDto dto);
}

public class ProjectRepository : IProjectRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public ProjectRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<ProjectDto?> GetByIdAsync(Guid id)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        const string sql = """
            SELECT p."Id", p."ClientId", p."FreelancerId", p."CategoryId",
                   p."Title", p."Slug", p."Description", p."Requirements", p."Status",
                   p."BudgetType", p."BudgetMin", p."BudgetMax",
                   p."AgreedBudget", p."Currency",
                   p."EstimatedDurationDays", p."Deadline",
                   p."RequiredSkills", p."ExperienceLevel",
                   p."ProjectType", p."Visibility", p."BidCount",
                   p."ViewCount", p."IsUrgent", p."IsFeatured",
                   p."Tags", p."CreatedAt",
                   c."FirstName" || ' ' || c."LastName" as ClientName, c."AvatarUrl" as ClientAvatarUrl,
                   f."FirstName" || ' ' || f."LastName" as FreelancerName,
                   cat."Name" as CategoryName
            FROM projects p
            JOIN users c ON p."ClientId" = c."Id"
            LEFT JOIN users f ON p."FreelancerId" = f."Id"
            JOIN "Categories" cat ON p."CategoryId" = cat."Id"
            WHERE p."Id" = @Id AND p."IsDeleted" = false
            """;

        return await connection.QuerySingleOrDefaultAsync<ProjectDto>(sql, new { Id = id });
    }

    public async Task<(IEnumerable<ProjectListDto> Projects, int TotalCount)> GetAllAsync(ProjectQueryParams query)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        var whereClause = "WHERE p.\"IsDeleted\" = false";
        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(query.Search))
        {
            whereClause += " AND (p.\"Title\" ILIKE @Search OR p.\"Description\" ILIKE @Search OR p.\"Tags\" ILIKE @Search)";
            parameters.Add("Search", $"%{query.Search}%");
        }
        if (!string.IsNullOrEmpty(query.Status))
        {
            whereClause += " AND p.\"Status\" = @Status::integer";
            parameters.Add("Status", query.Status);
        }
        if (query.CategoryId.HasValue)
        {
            whereClause += " AND p.\"CategoryId\" = @CategoryId";
            parameters.Add("CategoryId", query.CategoryId);
        }
        if (query.MinBudget.HasValue)
        {
            whereClause += " AND p.\"BudgetMax\" >= @MinBudget";
            parameters.Add("MinBudget", query.MinBudget);
        }
        if (query.MaxBudget.HasValue)
        {
            whereClause += " AND p.\"BudgetMin\" <= @MaxBudget";
            parameters.Add("MaxBudget", query.MaxBudget);
        }

        var countSql = $"SELECT COUNT(*) FROM projects p {whereClause}";
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

        parameters.Add("PageSize", query.PageSize);
        parameters.Add("Offset", (query.Page - 1) * query.PageSize);

        var sql = $"""
            SELECT p."Id", p."Title", p."Slug", p."Status",
                   p."BudgetType", p."BudgetMin", p."BudgetMax",
                   p."Currency", p."Deadline", p."BidCount",
                   p."IsUrgent", p."IsFeatured",
                   p."ExperienceLevel", p."RequiredSkills",
                   p."CreatedAt",
                   c."FirstName" || ' ' || c."LastName" as ClientName,
                   cat."Name" as CategoryName
            FROM projects p
            JOIN users c ON p."ClientId" = c."Id"
            JOIN "Categories" cat ON p."CategoryId" = cat."Id"
            {whereClause}
            ORDER BY p."IsFeatured" DESC, p."IsUrgent" DESC, p."CreatedAt" DESC
            LIMIT @PageSize OFFSET @Offset
            """;

        var projects = await connection.QueryAsync<ProjectListDto>(sql, parameters);
        return (projects, totalCount);
    }

    public async Task<(IEnumerable<ProjectListDto> Projects, int TotalCount)> GetByClientAsync(Guid clientId, int page, int pageSize)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        var totalCount = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM projects WHERE \"ClientId\" = @ClientId AND \"IsDeleted\" = false",
            new { ClientId = clientId });

        var sql = """
            SELECT p."Id", p."Title", p."Slug", p."Status",
                   p."BudgetType", p."BudgetMin", p."BudgetMax",
                   p."Currency", p."Deadline", p."BidCount",
                   p."IsUrgent", p."CreatedAt",
                   cat."Name" as CategoryName
            FROM projects p
            JOIN "Categories" cat ON p."CategoryId" = cat."Id"
            WHERE p."ClientId" = @ClientId AND p."IsDeleted" = false
            ORDER BY p."CreatedAt" DESC
            LIMIT @PageSize OFFSET @Offset
            """;

        var projects = await connection.QueryAsync<ProjectListDto>(sql, new
        {
            ClientId = clientId,
            PageSize = pageSize,
            Offset = (page - 1) * pageSize
        });

        return (projects, totalCount);
    }

    public async Task<(IEnumerable<ProjectListDto> Projects, int TotalCount)> GetByFreelancerAsync(Guid freelancerId, int page, int pageSize)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        var totalCount = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM projects WHERE \"FreelancerId\" = @FreelancerId AND \"IsDeleted\" = false",
            new { FreelancerId = freelancerId });

        var sql = """
            SELECT p."Id", p."Title", p."Slug", p."Status",
                   p."BudgetType", p."BudgetMin", p."BudgetMax",
                   p."Currency", p."Deadline", p."BidCount", p."CreatedAt",
                   c."FirstName" || ' ' || c."LastName" as ClientName,
                   cat."Name" as CategoryName
            FROM projects p
            JOIN users c ON p."ClientId" = c."Id"
            JOIN "Categories" cat ON p."CategoryId" = cat."Id"
            WHERE p."FreelancerId" = @FreelancerId AND p."IsDeleted" = false
            ORDER BY p."CreatedAt" DESC
            LIMIT @PageSize OFFSET @Offset
            """;

        var projects = await connection.QueryAsync<ProjectListDto>(sql, new
        {
            FreelancerId = freelancerId,
            PageSize = pageSize,
            Offset = (page - 1) * pageSize
        });

        return (projects, totalCount);
    }

    public async Task<Guid> CreateAsync(CreateProjectDto dto, Guid clientId)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();

        var id = Guid.NewGuid();
        var slug = dto.Title.ToLower().Replace(" ", "-").Replace("'", "").Replace("\"", "");

        const string sql = """
            INSERT INTO projects ("Id", "ClientId", "CategoryId", "Title", "Slug", "Description", "Requirements",
                                "Status", "BudgetType", "BudgetMin", "BudgetMax", "Currency",
                                "EstimatedDurationDays", "Deadline", "RequiredSkills", "ExperienceLevel",
                                "ProjectType", "Visibility", "BidCount", "ViewCount",
                                "IsUrgent", "IsFeatured", "Tags", "CreatedAt", "IsDeleted")
            VALUES (@Id, @ClientId, @CategoryId, @Title, @Slug, @Description, @Requirements,
                   @Status, @BudgetType, @BudgetMin, @BudgetMax, @Currency,
                   @EstimatedDurationDays, @Deadline, @RequiredSkills, @ExperienceLevel,
                   @ProjectType, @Visibility, 0, 0,
                   @IsUrgent, false, @Tags, NOW(), false)
            RETURNING "Id"
            """;

        return await connection.ExecuteScalarAsync<Guid>(sql, new
        {
            Id = id,
            ClientId = clientId,
            dto.CategoryId,
            dto.Title,
            Slug = slug,
            dto.Description,
            dto.Requirements,
            Status = (int)ProjectStatus.Open,
            BudgetType = dto.BudgetType ?? "Fixed",
            dto.BudgetMin,
            dto.BudgetMax,
            Currency = dto.Currency ?? "USD",
            dto.EstimatedDurationDays,
            dto.Deadline,
            dto.RequiredSkills,
            ExperienceLevel = dto.ExperienceLevel ?? "Intermediate",
            ProjectType = dto.ProjectType ?? "One-time",
            Visibility = dto.Visibility ?? "Public",
            IsUrgent = dto.IsUrgent ?? false,
            dto.Tags
        });
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateProjectDto dto)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();

        var updates = new List<string>();
        var parameters = new DynamicParameters();
        parameters.Add("Id", id);

        if (dto.Title != null) { updates.Add("\"Title\" = @Title"); parameters.Add("Title", dto.Title); }
        if (dto.Description != null) { updates.Add("\"Description\" = @Description"); parameters.Add("Description", dto.Description); }
        if (dto.Requirements != null) { updates.Add("\"Requirements\" = @Requirements"); parameters.Add("Requirements", dto.Requirements); }
        if (dto.Status.HasValue) { updates.Add("\"Status\" = @Status"); parameters.Add("Status", dto.Status); }
        if (dto.BudgetMin.HasValue) { updates.Add("\"BudgetMin\" = @BudgetMin"); parameters.Add("BudgetMin", dto.BudgetMin); }
        if (dto.BudgetMax.HasValue) { updates.Add("\"BudgetMax\" = @BudgetMax"); parameters.Add("BudgetMax", dto.BudgetMax); }
        if (dto.Deadline.HasValue) { updates.Add("\"Deadline\" = @Deadline"); parameters.Add("Deadline", dto.Deadline); }

        if (updates.Count == 0) return true;
        updates.Add("\"UpdatedAt\" = NOW()");

        var sql = $"UPDATE projects SET {string.Join(", ", updates)} WHERE \"Id\" = @Id AND \"IsDeleted\" = false";
        return await connection.ExecuteAsync(sql, parameters) > 0;
    }

    public async Task<IEnumerable<ProjectBidDto>> GetBidsAsync(Guid projectId, int page, int pageSize)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        const string sql = """
            SELECT b."Id", b."ProjectId", b."FreelancerId",
                   b."Amount", b."Currency", b."DeliveryDays",
                   b."Proposal", b."Status", b."IsShortlisted",
                   b."CreatedAt",
                   u."FirstName" || ' ' || u."LastName" as FreelancerName,
                   u."AvatarUrl" as FreelancerAvatarUrl,
                   up."HourlyRate" as FreelancerHourlyRate,
                   u."AverageRating" as FreelancerRating
            FROM project_bids b
            JOIN users u ON b."FreelancerId" = u."Id"
            LEFT JOIN user_profiles up ON u."Id" = up."UserId"
            WHERE b."ProjectId" = @ProjectId AND b."IsDeleted" = false
            ORDER BY b."IsShortlisted" DESC, b."CreatedAt" DESC
            LIMIT @PageSize OFFSET @Offset
            """;

        return await connection.QueryAsync<ProjectBidDto>(sql, new
        {
            ProjectId = projectId,
            PageSize = pageSize,
            Offset = (page - 1) * pageSize
        });
    }

    public async Task<Guid> SubmitBidAsync(CreateBidDto dto, Guid freelancerId)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();

        var id = Guid.NewGuid();

        await connection.ExecuteAsync("""
            INSERT INTO project_bids ("Id", "ProjectId", "FreelancerId", "Amount", "Currency", "DeliveryDays",
                                     "Proposal", "Status", "IsShortlisted", "CreatedAt", "IsDeleted")
            VALUES (@Id, @ProjectId, @FreelancerId, @Amount, @Currency, @DeliveryDays,
                   @Proposal, @Status, false, NOW(), false)
            """, new
        {
            Id = id,
            dto.ProjectId,
            FreelancerId = freelancerId,
            dto.Amount,
            Currency = dto.Currency ?? "USD",
            dto.DeliveryDays,
            dto.Proposal,
            Status = (int)BidStatus.Submitted
        });

        await connection.ExecuteAsync(
            "UPDATE projects SET \"BidCount\" = \"BidCount\" + 1, \"UpdatedAt\" = NOW() WHERE \"Id\" = @ProjectId",
            new { dto.ProjectId });

        return id;
    }

    public async Task<IEnumerable<ProjectMilestoneDto>> GetMilestonesAsync(Guid projectId)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        const string sql = """
            SELECT "Id", "ProjectId", "Title", "Description", "Amount", "Currency",
                   "DueDate", "Status", "IsFunded", "IsReleased",
                   "CreatedAt"
            FROM project_milestones
            WHERE "ProjectId" = @ProjectId AND "IsDeleted" = false
            ORDER BY "DueDate"
            """;

        return await connection.QueryAsync<ProjectMilestoneDto>(sql, new { ProjectId = projectId });
    }

    public async Task<Guid> CreateMilestoneAsync(CreateMilestoneDto dto)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();

        var id = Guid.NewGuid();

        await connection.ExecuteAsync("""
            INSERT INTO project_milestones ("Id", "ProjectId", "Title", "Description", "Amount", "Currency",
                                           "DueDate", "Status", "SortOrder", "IsFunded", "IsReleased",
                                           "CreatedAt", "IsDeleted")
            VALUES (@Id, @ProjectId, @Title, @Description, @Amount, @Currency,
                   @DueDate, @Status, 0, false, false,
                   NOW(), false)
            """, new
        {
            Id = id,
            dto.ProjectId,
            dto.Title,
            dto.Description,
            dto.Amount,
            Currency = dto.Currency ?? "USD",
            dto.DueDate,
            Status = (int)MilestoneStatus.Pending
        });

        return id;
    }
}

// DTOs
public record ProjectDto
{
    public Guid Id { get; init; }
    public Guid ClientId { get; init; }
    public Guid? FreelancerId { get; init; }
    public Guid CategoryId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Requirements { get; init; }
    public int Status { get; init; }
    public string BudgetType { get; init; } = "Fixed";
    public decimal? BudgetMin { get; init; }
    public decimal? BudgetMax { get; init; }
    public decimal? AgreedBudget { get; init; }
    public string Currency { get; init; } = "USD";
    public int? EstimatedDurationDays { get; init; }
    public DateTime? Deadline { get; init; }
    public string? RequiredSkills { get; init; }
    public string? ExperienceLevel { get; init; }
    public string? ProjectType { get; init; }
    public string? Visibility { get; init; }
    public int BidCount { get; init; }
    public int ViewCount { get; init; }
    public bool IsUrgent { get; init; }
    public bool IsFeatured { get; init; }
    public string? Tags { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? ClientName { get; init; }
    public string? ClientAvatarUrl { get; init; }
    public string? FreelancerName { get; init; }
    public string? CategoryName { get; init; }
}

public record ProjectListDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public int Status { get; init; }
    public string BudgetType { get; init; } = "Fixed";
    public decimal? BudgetMin { get; init; }
    public decimal? BudgetMax { get; init; }
    public string Currency { get; init; } = "USD";
    public DateTime? Deadline { get; init; }
    public int BidCount { get; init; }
    public bool IsUrgent { get; init; }
    public bool IsFeatured { get; init; }
    public string? ExperienceLevel { get; init; }
    public string? RequiredSkills { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? ClientName { get; init; }
    public string? CategoryName { get; init; }
}

public record CreateProjectDto(
    Guid CategoryId,
    string Title,
    string? Description = null,
    string? Requirements = null,
    string? BudgetType = "Fixed",
    decimal? BudgetMin = null,
    decimal? BudgetMax = null,
    string? Currency = "USD",
    int? EstimatedDurationDays = null,
    DateTime? Deadline = null,
    string? RequiredSkills = null,
    string? ExperienceLevel = "Intermediate",
    string? ProjectType = "One-time",
    string? Visibility = "Public",
    bool? IsUrgent = false,
    string? Tags = null);

public record UpdateProjectDto(
    string? Title = null,
    string? Description = null,
    string? Requirements = null,
    int? Status = null,
    decimal? BudgetMin = null,
    decimal? BudgetMax = null,
    DateTime? Deadline = null);

public record CreateBidDto(
    Guid ProjectId,
    decimal Amount,
    string? Currency = "USD",
    int DeliveryDays = 7,
    string? Proposal = null);

public record ProjectBidDto
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public Guid FreelancerId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "USD";
    public int DeliveryDays { get; init; }
    public string? Proposal { get; init; }
    public int Status { get; init; }
    public bool IsShortlisted { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? FreelancerName { get; init; }
    public string? FreelancerAvatarUrl { get; init; }
    public decimal? FreelancerHourlyRate { get; init; }
    public decimal? FreelancerRating { get; init; }
}

public record CreateMilestoneDto(
    Guid ProjectId,
    string Title,
    string? Description = null,
    decimal Amount = 0,
    string? Currency = "USD",
    DateTime? DueDate = null);

public record ProjectMilestoneDto
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "USD";
    public DateTime? DueDate { get; init; }
    public int Status { get; init; }
    public bool IsFunded { get; init; }
    public bool IsReleased { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record ProjectQueryParams
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Search { get; init; }
    public string? Status { get; init; }
    public Guid? CategoryId { get; init; }
    public decimal? MinBudget { get; init; }
    public decimal? MaxBudget { get; init; }
    public string? ExperienceLevel { get; init; }
}
