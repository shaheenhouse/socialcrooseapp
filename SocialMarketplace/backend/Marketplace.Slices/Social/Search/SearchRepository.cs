using Dapper;
using Marketplace.Core.Infrastructure;
using Marketplace.Database.Entities.Social;

namespace Marketplace.Slices.Social.Search;

public interface ISearchRepository
{
    Task<IEnumerable<UserSearchResult>> SearchUsersAsync(string query, SearchFilters filters, int page, int pageSize);
    Task<IEnumerable<ServiceSearchResult>> SearchServicesAsync(string query, SearchFilters filters, int page, int pageSize);
    Task<IEnumerable<ProjectSearchResult>> SearchProjectsAsync(string query, SearchFilters filters, int page, int pageSize);
    Task<IEnumerable<CompanySearchResult>> SearchCompaniesAsync(string query, SearchFilters filters, int page, int pageSize);
    Task<IEnumerable<PostSearchResult>> SearchPostsAsync(string query, SearchFilters filters, int page, int pageSize);
    Task SaveSearchHistoryAsync(SearchHistory history);
    Task<IEnumerable<SearchHistory>> GetRecentSearchesAsync(Guid userId, int limit);
    Task DeleteSearchHistoryAsync(Guid userId, Guid? searchId = null);
    Task<IEnumerable<TrendingSearch>> GetTrendingSearchesAsync(SearchType? type, int limit);
}

public class SearchRepository : ISearchRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public SearchRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<UserSearchResult>> SearchUsersAsync(string query, SearchFilters filters, int page, int pageSize)
    {
        using var connection = _connectionFactory.CreateReadConnection();
        var offset = (page - 1) * pageSize;
        var searchPattern = $"%{query}%";

        return await connection.QueryAsync<UserSearchResult>(
            @"SELECT u.""Id"", u.""FirstName"" || ' ' || u.""LastName"" as DisplayName,
                     u.""Email"", u.""AvatarUrl"", up.""Headline"",
                     u.""City"" || ', ' || u.""Country"" as Location,
                     (SELECT COUNT(*) FROM connections WHERE (requester_id = u.""Id"" OR addressee_id = u.""Id"") AND status = 1) as ConnectionCount
              FROM users u
              LEFT JOIN user_profiles up ON up.""UserId"" = u.""Id"" AND up.""IsDeleted"" = false
              WHERE u.""IsDeleted"" = false AND u.""Status"" = 1
              AND (u.""FirstName"" ILIKE @Pattern OR u.""LastName"" ILIKE @Pattern OR u.""Email"" ILIKE @Pattern OR up.""Headline"" ILIKE @Pattern)
              AND (@Location IS NULL OR u.""City"" ILIKE @LocationPattern OR u.""Country"" ILIKE @LocationPattern)
              AND (@Skills IS NULL OR EXISTS (
                  SELECT 1 FROM user_skills us 
                  INNER JOIN skills s ON us.""SkillId"" = s.""Id"" 
                  WHERE us.""UserId"" = u.""Id"" AND s.""Name"" = ANY(@Skills)
              ))
              ORDER BY 
                  CASE WHEN u.""FirstName"" ILIKE @Pattern THEN 0 ELSE 1 END,
                  ConnectionCount DESC
              LIMIT @PageSize OFFSET @Offset",
            new 
            { 
                Pattern = searchPattern, 
                Location = filters.Location,
                LocationPattern = filters.Location != null ? $"%{filters.Location}%" : null,
                Skills = filters.Skills?.ToArray(),
                PageSize = pageSize, 
                Offset = offset 
            });
    }

    public async Task<IEnumerable<ServiceSearchResult>> SearchServicesAsync(string query, SearchFilters filters, int page, int pageSize)
    {
        using var connection = _connectionFactory.CreateReadConnection();
        var offset = (page - 1) * pageSize;
        var searchPattern = $"%{query}%";

        return await connection.QueryAsync<ServiceSearchResult>(
            @"SELECT s.""Id"", s.""Title"", s.""Description"", s.""BasePrice"" as Price, s.""Currency"", s.""DeliveryTime"" as DeliveryDays,
                     s.""Rating"", s.""TotalReviews"" as ReviewCount, s.""TotalOrders"" as OrderCount,
                     s.""SellerId"", u.""FirstName"" || ' ' || u.""LastName"" as SellerName
              FROM ""Services"" s
              INNER JOIN users u ON s.""SellerId"" = u.""Id""
              WHERE s.""IsDeleted"" = false AND s.""Status"" = 1
              AND (s.""Title"" ILIKE @Pattern OR s.""Description"" ILIKE @Pattern)
              AND (@MinPrice IS NULL OR s.""BasePrice"" >= @MinPrice)
              AND (@MaxPrice IS NULL OR s.""BasePrice"" <= @MaxPrice)
              AND (@Category IS NULL OR s.""CategoryId"" = @Category)
              ORDER BY 
                  CASE WHEN s.""Title"" ILIKE @Pattern THEN 0 ELSE 1 END,
                  s.""Rating"" DESC, s.""TotalOrders"" DESC
              LIMIT @PageSize OFFSET @Offset",
            new
            {
                Pattern = searchPattern,
                filters.MinPrice,
                filters.MaxPrice,
                Category = filters.CategoryId,
                PageSize = pageSize,
                Offset = offset
            });
    }

    public async Task<IEnumerable<ProjectSearchResult>> SearchProjectsAsync(string query, SearchFilters filters, int page, int pageSize)
    {
        using var connection = _connectionFactory.CreateReadConnection();
        var offset = (page - 1) * pageSize;
        var searchPattern = $"%{query}%";

        return await connection.QueryAsync<ProjectSearchResult>(
            @"SELECT p.""Id"", p.""Title"", p.""Description"", p.""BudgetMin"", p.""BudgetMax"", p.""Currency"",
                     p.""Status"", p.""BidCount"", p.""CreatedAt"", p.""Deadline""
              FROM projects p
              WHERE p.""IsDeleted"" = false AND p.""Status"" != 0
              AND (p.""Title"" ILIKE @Pattern OR p.""Description"" ILIKE @Pattern)
              AND (@MinBudget IS NULL OR p.""BudgetMax"" >= @MinBudget)
              AND (@MaxBudget IS NULL OR p.""BudgetMin"" <= @MaxBudget)
              AND (@Status IS NULL OR p.""Status"" = @Status)
              ORDER BY 
                  CASE WHEN p.""Title"" ILIKE @Pattern THEN 0 ELSE 1 END,
                  p.""CreatedAt"" DESC
              LIMIT @PageSize OFFSET @Offset",
            new
            {
                Pattern = searchPattern,
                MinBudget = filters.MinPrice,
                MaxBudget = filters.MaxPrice,
                filters.Status,
                PageSize = pageSize,
                Offset = offset
            });
    }

    public async Task<IEnumerable<CompanySearchResult>> SearchCompaniesAsync(string query, SearchFilters filters, int page, int pageSize)
    {
        using var connection = _connectionFactory.CreateReadConnection();
        var offset = (page - 1) * pageSize;
        var searchPattern = $"%{query}%";

        return await connection.QueryAsync<CompanySearchResult>(
            @"SELECT p.id, p.name, p.slug, p.description, p.logo_url as LogoUrl, p.industry,
                     p.headquarters, p.employee_count as EmployeeCount, p.follower_count as FollowerCount, p.is_verified as IsVerified
              FROM pages p
              WHERE p.is_active = true AND p.type IN (0, 1, 3)
              AND (p.name ILIKE @Pattern OR p.description ILIKE @Pattern OR p.industry ILIKE @Pattern)
              AND (@Industry IS NULL OR p.industry ILIKE @IndustryPattern)
              AND (@Location IS NULL OR p.headquarters ILIKE @LocationPattern)
              ORDER BY 
                  CASE WHEN p.name ILIKE @Pattern THEN 0 ELSE 1 END,
                  p.follower_count DESC
              LIMIT @PageSize OFFSET @Offset",
            new
            {
                Pattern = searchPattern,
                Industry = filters.Industry,
                IndustryPattern = filters.Industry != null ? $"%{filters.Industry}%" : null,
                Location = filters.Location,
                LocationPattern = filters.Location != null ? $"%{filters.Location}%" : null,
                PageSize = pageSize,
                Offset = offset
            });
    }

    public async Task<IEnumerable<PostSearchResult>> SearchPostsAsync(string query, SearchFilters filters, int page, int pageSize)
    {
        using var connection = _connectionFactory.CreateReadConnection();
        var offset = (page - 1) * pageSize;
        var searchPattern = $"%{query}%";

        return await connection.QueryAsync<PostSearchResult>(
            @"SELECT p.id, p.content, p.type, p.author_id as AuthorId,
                     u.""FirstName"" || ' ' || u.""LastName"" as AuthorName,
                     p.like_count as LikeCount, p.comment_count as CommentCount, p.share_count as ShareCount, p.created_at as CreatedAt
              FROM posts p
              INNER JOIN users u ON p.author_id = u.""Id""
              WHERE p.is_active = true AND p.visibility = 0
              AND p.content ILIKE @Pattern
              ORDER BY p.created_at DESC
              LIMIT @PageSize OFFSET @Offset",
            new { Pattern = searchPattern, PageSize = pageSize, Offset = offset });
    }

    public async Task SaveSearchHistoryAsync(SearchHistory history)
    {
        using var connection = _connectionFactory.CreateWriteConnection();
        await connection.ExecuteAsync(
            @"INSERT INTO search_history (id, user_id, query, type, filters, result_count, clicked_result_id, created_at)
              VALUES (@Id, @UserId, @Query, @Type, @Filters::jsonb, @ResultCount, @ClickedResultId, @CreatedAt)",
            new
            {
                Id = Guid.NewGuid(),
                history.UserId,
                history.Query,
                history.Type,
                history.Filters,
                history.ResultCount,
                history.ClickedResultId,
                CreatedAt = DateTime.UtcNow
            });
    }

    public async Task<IEnumerable<SearchHistory>> GetRecentSearchesAsync(Guid userId, int limit)
    {
        using var connection = _connectionFactory.CreateReadConnection();
        return await connection.QueryAsync<SearchHistory>(
            @"SELECT DISTINCT ON (query) * FROM search_history
              WHERE user_id = @UserId
              ORDER BY query, created_at DESC
              LIMIT @Limit",
            new { UserId = userId, Limit = limit });
    }

    public async Task DeleteSearchHistoryAsync(Guid userId, Guid? searchId = null)
    {
        using var connection = _connectionFactory.CreateWriteConnection();
        if (searchId.HasValue)
        {
            await connection.ExecuteAsync(
                "DELETE FROM search_history WHERE user_id = @UserId AND id = @SearchId",
                new { UserId = userId, SearchId = searchId });
        }
        else
        {
            await connection.ExecuteAsync(
                "DELETE FROM search_history WHERE user_id = @UserId",
                new { UserId = userId });
        }
    }

    public async Task<IEnumerable<TrendingSearch>> GetTrendingSearchesAsync(SearchType? type, int limit)
    {
        using var connection = _connectionFactory.CreateReadConnection();
        var now = DateTime.UtcNow;
        return await connection.QueryAsync<TrendingSearch>(
            @"SELECT * FROM trending_searches
              WHERE (@Type IS NULL OR type = @Type)
              AND period_end >= @Now
              ORDER BY rank
              LIMIT @Limit",
            new { Type = type, Now = now, Limit = limit });
    }
}

public record UserSearchResult
{
    public Guid Id { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? AvatarUrl { get; init; }
    public string? Headline { get; init; }
    public string? Location { get; init; }
    public int ConnectionCount { get; init; }
}

public record ServiceSearchResult
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public string Currency { get; init; } = "USD";
    public int DeliveryDays { get; init; }
    public decimal Rating { get; init; }
    public int ReviewCount { get; init; }
    public int OrderCount { get; init; }
    public Guid SellerId { get; init; }
    public string SellerName { get; init; } = string.Empty;
}

public record ProjectSearchResult
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal BudgetMin { get; init; }
    public decimal BudgetMax { get; init; }
    public string Currency { get; init; } = "USD";
    public string Status { get; init; } = string.Empty;
    public int BidCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? Deadline { get; init; }
}

public record CompanySearchResult
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? LogoUrl { get; init; }
    public string? Industry { get; init; }
    public string? Headquarters { get; init; }
    public int EmployeeCount { get; init; }
    public int FollowerCount { get; init; }
    public bool IsVerified { get; init; }
}

public record PostSearchResult
{
    public Guid Id { get; init; }
    public string Content { get; init; } = string.Empty;
    public PostType Type { get; init; }
    public Guid AuthorId { get; init; }
    public string AuthorName { get; init; } = string.Empty;
    public int LikeCount { get; init; }
    public int CommentCount { get; init; }
    public int ShareCount { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record SearchFilters
{
    public string? Location { get; init; }
    public string? Industry { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public Guid? CategoryId { get; init; }
    public string? Status { get; init; }
    public IEnumerable<string>? Skills { get; init; }
}
