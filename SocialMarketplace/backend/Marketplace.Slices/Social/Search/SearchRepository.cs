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
            @"SELECT u.id, u.display_name, u.email, u.avatar_url, u.headline, u.location,
                     (SELECT COUNT(*) FROM connections WHERE (requester_id = u.id OR addressee_id = u.id) AND status = 1) as connection_count
              FROM users u
              WHERE u.is_active = true
              AND (u.display_name ILIKE @Pattern OR u.headline ILIKE @Pattern OR u.email ILIKE @Pattern)
              AND (@Location IS NULL OR u.location ILIKE @LocationPattern)
              AND (@Skills IS NULL OR EXISTS (
                  SELECT 1 FROM user_skills us 
                  INNER JOIN skills s ON us.skill_id = s.id 
                  WHERE us.user_id = u.id AND s.name = ANY(@Skills)
              ))
              ORDER BY 
                  CASE WHEN u.display_name ILIKE @Pattern THEN 0 ELSE 1 END,
                  connection_count DESC
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
            @"SELECT s.id, s.title, s.description, s.price, s.currency, s.delivery_days,
                     s.rating, s.review_count, s.order_count, s.seller_id, u.display_name as seller_name
              FROM services s
              INNER JOIN users u ON s.seller_id = u.id
              WHERE s.is_active = true
              AND (s.title ILIKE @Pattern OR s.description ILIKE @Pattern)
              AND (@MinPrice IS NULL OR s.price >= @MinPrice)
              AND (@MaxPrice IS NULL OR s.price <= @MaxPrice)
              AND (@Category IS NULL OR s.category_id = @Category)
              ORDER BY 
                  CASE WHEN s.title ILIKE @Pattern THEN 0 ELSE 1 END,
                  s.rating DESC, s.order_count DESC
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
            @"SELECT p.id, p.title, p.description, p.budget_min, p.budget_max, p.currency,
                     p.status, p.bid_count, p.created_at, p.deadline
              FROM projects p
              WHERE p.is_active = true
              AND (p.title ILIKE @Pattern OR p.description ILIKE @Pattern)
              AND (@MinBudget IS NULL OR p.budget_max >= @MinBudget)
              AND (@MaxBudget IS NULL OR p.budget_min <= @MaxBudget)
              AND (@Status IS NULL OR p.status = @Status)
              ORDER BY 
                  CASE WHEN p.title ILIKE @Pattern THEN 0 ELSE 1 END,
                  p.created_at DESC
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
            @"SELECT p.id, p.name, p.slug, p.description, p.logo_url, p.industry,
                     p.headquarters, p.employee_count, p.follower_count, p.is_verified
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
            @"SELECT p.id, p.content, p.type, p.author_id, u.display_name as author_name,
                     p.like_count, p.comment_count, p.share_count, p.created_at
              FROM posts p
              INNER JOIN users u ON p.author_id = u.id
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
