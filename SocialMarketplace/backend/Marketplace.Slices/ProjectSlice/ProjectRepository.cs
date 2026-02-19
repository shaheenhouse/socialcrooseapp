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
            SELECT p.id, p.client_id as ClientId, p.freelancer_id as FreelancerId, p.category_id as CategoryId,
                   p.title, p.slug, p.description, p.requirements, p.status,
                   p.budget_type as BudgetType, p.budget_min as BudgetMin, p.budget_max as BudgetMax,
                   p.agreed_budget as AgreedBudget, p.currency,
                   p.estimated_duration_days as EstimatedDurationDays, p.deadline,
                   p.required_skills as RequiredSkills, p.experience_level as ExperienceLevel,
                   p.project_type as ProjectType, p.visibility, p.bid_count as BidCount,
                   p.view_count as ViewCount, p.is_urgent as IsUrgent, p.is_featured as IsFeatured,
                   p.tags, p.created_at as CreatedAt,
                   c.first_name || ' ' || c.last_name as ClientName, c.avatar_url as ClientAvatarUrl,
                   f.first_name || ' ' || f.last_name as FreelancerName,
                   cat.name as CategoryName
            FROM projects p
            JOIN users c ON p.client_id = c.id
            LEFT JOIN users f ON p.freelancer_id = f.id
            JOIN categories cat ON p.category_id = cat.id
            WHERE p.id = @Id AND p.is_deleted = false
            """;

        return await connection.QuerySingleOrDefaultAsync<ProjectDto>(sql, new { Id = id });
    }

    public async Task<(IEnumerable<ProjectListDto> Projects, int TotalCount)> GetAllAsync(ProjectQueryParams query)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        var whereClause = "WHERE p.is_deleted = false";
        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(query.Search))
        {
            whereClause += " AND (p.title ILIKE @Search OR p.description ILIKE @Search OR p.tags ILIKE @Search)";
            parameters.Add("Search", $"%{query.Search}%");
        }
        if (!string.IsNullOrEmpty(query.Status))
        {
            whereClause += " AND p.status = @Status::integer";
            parameters.Add("Status", query.Status);
        }
        if (query.CategoryId.HasValue)
        {
            whereClause += " AND p.category_id = @CategoryId";
            parameters.Add("CategoryId", query.CategoryId);
        }
        if (query.MinBudget.HasValue)
        {
            whereClause += " AND p.budget_max >= @MinBudget";
            parameters.Add("MinBudget", query.MinBudget);
        }
        if (query.MaxBudget.HasValue)
        {
            whereClause += " AND p.budget_min <= @MaxBudget";
            parameters.Add("MaxBudget", query.MaxBudget);
        }

        var countSql = $"SELECT COUNT(*) FROM projects p {whereClause}";
        var totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

        parameters.Add("PageSize", query.PageSize);
        parameters.Add("Offset", (query.Page - 1) * query.PageSize);

        var sql = $"""
            SELECT p.id, p.title, p.slug, p.status,
                   p.budget_type as BudgetType, p.budget_min as BudgetMin, p.budget_max as BudgetMax,
                   p.currency, p.deadline, p.bid_count as BidCount,
                   p.is_urgent as IsUrgent, p.is_featured as IsFeatured,
                   p.experience_level as ExperienceLevel, p.required_skills as RequiredSkills,
                   p.created_at as CreatedAt,
                   c.first_name || ' ' || c.last_name as ClientName,
                   cat.name as CategoryName
            FROM projects p
            JOIN users c ON p.client_id = c.id
            JOIN categories cat ON p.category_id = cat.id
            {whereClause}
            ORDER BY p.is_featured DESC, p.is_urgent DESC, p.created_at DESC
            LIMIT @PageSize OFFSET @Offset
            """;

        var projects = await connection.QueryAsync<ProjectListDto>(sql, parameters);
        return (projects, totalCount);
    }

    public async Task<(IEnumerable<ProjectListDto> Projects, int TotalCount)> GetByClientAsync(Guid clientId, int page, int pageSize)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        var totalCount = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM projects WHERE client_id = @ClientId AND is_deleted = false",
            new { ClientId = clientId });

        var sql = """
            SELECT p.id, p.title, p.slug, p.status,
                   p.budget_type as BudgetType, p.budget_min as BudgetMin, p.budget_max as BudgetMax,
                   p.currency, p.deadline, p.bid_count as BidCount,
                   p.is_urgent as IsUrgent, p.created_at as CreatedAt,
                   cat.name as CategoryName
            FROM projects p
            JOIN categories cat ON p.category_id = cat.id
            WHERE p.client_id = @ClientId AND p.is_deleted = false
            ORDER BY p.created_at DESC
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
            "SELECT COUNT(*) FROM projects WHERE freelancer_id = @FreelancerId AND is_deleted = false",
            new { FreelancerId = freelancerId });

        var sql = """
            SELECT p.id, p.title, p.slug, p.status,
                   p.budget_type as BudgetType, p.budget_min as BudgetMin, p.budget_max as BudgetMax,
                   p.currency, p.deadline, p.bid_count as BidCount, p.created_at as CreatedAt,
                   c.first_name || ' ' || c.last_name as ClientName,
                   cat.name as CategoryName
            FROM projects p
            JOIN users c ON p.client_id = c.id
            JOIN categories cat ON p.category_id = cat.id
            WHERE p.freelancer_id = @FreelancerId AND p.is_deleted = false
            ORDER BY p.created_at DESC
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
            INSERT INTO projects (id, client_id, category_id, title, slug, description, requirements,
                                status, budget_type, budget_min, budget_max, currency,
                                estimated_duration_days, deadline, required_skills, experience_level,
                                project_type, visibility, is_urgent, tags, created_at, is_deleted)
            VALUES (@Id, @ClientId, @CategoryId, @Title, @Slug, @Description, @Requirements,
                   @Status, @BudgetType, @BudgetMin, @BudgetMax, @Currency,
                   @EstimatedDurationDays, @Deadline, @RequiredSkills, @ExperienceLevel,
                   @ProjectType, @Visibility, @IsUrgent, @Tags, NOW(), false)
            RETURNING id
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

        if (dto.Title != null) { updates.Add("title = @Title"); parameters.Add("Title", dto.Title); }
        if (dto.Description != null) { updates.Add("description = @Description"); parameters.Add("Description", dto.Description); }
        if (dto.Requirements != null) { updates.Add("requirements = @Requirements"); parameters.Add("Requirements", dto.Requirements); }
        if (dto.Status.HasValue) { updates.Add("status = @Status"); parameters.Add("Status", dto.Status); }
        if (dto.BudgetMin.HasValue) { updates.Add("budget_min = @BudgetMin"); parameters.Add("BudgetMin", dto.BudgetMin); }
        if (dto.BudgetMax.HasValue) { updates.Add("budget_max = @BudgetMax"); parameters.Add("BudgetMax", dto.BudgetMax); }
        if (dto.Deadline.HasValue) { updates.Add("deadline = @Deadline"); parameters.Add("Deadline", dto.Deadline); }

        if (updates.Count == 0) return true;
        updates.Add("updated_at = NOW()");

        var sql = $"UPDATE projects SET {string.Join(", ", updates)} WHERE id = @Id AND is_deleted = false";
        return await connection.ExecuteAsync(sql, parameters) > 0;
    }

    public async Task<IEnumerable<ProjectBidDto>> GetBidsAsync(Guid projectId, int page, int pageSize)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        const string sql = """
            SELECT b.id, b.project_id as ProjectId, b.freelancer_id as FreelancerId,
                   b.amount, b.currency, b.delivery_days as DeliveryDays,
                   b.proposal, b.status, b.is_shortlisted as IsShortlisted,
                   b.created_at as CreatedAt,
                   u.first_name || ' ' || u.last_name as FreelancerName,
                   u.avatar_url as FreelancerAvatarUrl,
                   up.hourly_rate as FreelancerHourlyRate,
                   u.average_rating as FreelancerRating
            FROM project_bids b
            JOIN users u ON b.freelancer_id = u.id
            LEFT JOIN user_profiles up ON u.id = up.user_id
            WHERE b.project_id = @ProjectId AND b.is_deleted = false
            ORDER BY b.is_shortlisted DESC, b.created_at DESC
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
            INSERT INTO project_bids (id, project_id, freelancer_id, amount, currency, delivery_days,
                                     proposal, status, created_at, is_deleted)
            VALUES (@Id, @ProjectId, @FreelancerId, @Amount, @Currency, @DeliveryDays,
                   @Proposal, @Status, NOW(), false)
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
            "UPDATE projects SET bid_count = bid_count + 1, updated_at = NOW() WHERE id = @ProjectId",
            new { dto.ProjectId });

        return id;
    }

    public async Task<IEnumerable<ProjectMilestoneDto>> GetMilestonesAsync(Guid projectId)
    {
        using var connection = await _connectionFactory.CreateReadConnectionAsync();

        const string sql = """
            SELECT id, project_id as ProjectId, title, description, amount, currency,
                   due_date as DueDate, status, is_funded as IsFunded, is_released as IsReleased,
                   created_at as CreatedAt
            FROM project_milestones
            WHERE project_id = @ProjectId AND is_deleted = false
            ORDER BY due_date
            """;

        return await connection.QueryAsync<ProjectMilestoneDto>(sql, new { ProjectId = projectId });
    }

    public async Task<Guid> CreateMilestoneAsync(CreateMilestoneDto dto)
    {
        using var connection = await _connectionFactory.CreateWriteConnectionAsync();

        var id = Guid.NewGuid();

        await connection.ExecuteAsync("""
            INSERT INTO project_milestones (id, project_id, title, description, amount, currency,
                                           due_date, status, created_at, is_deleted)
            VALUES (@Id, @ProjectId, @Title, @Description, @Amount, @Currency,
                   @DueDate, @Status, NOW(), false)
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
