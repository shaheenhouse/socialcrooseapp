namespace Marketplace.Database.Entities.Social;

/// <summary>
/// User search history for personalization
/// </summary>
public class SearchHistory
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Query { get; set; } = string.Empty;
    public SearchType Type { get; set; }
    public string? Filters { get; set; } // JSON object of applied filters
    public int ResultCount { get; set; }
    public Guid? ClickedResultId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum SearchType
{
    All = 0,
    People = 1,
    Services = 2,
    Projects = 3,
    Companies = 4,
    Jobs = 5,
    Posts = 6,
    Tenders = 7
}

/// <summary>
/// Trending/popular searches
/// </summary>
public class TrendingSearch
{
    public Guid Id { get; set; }
    public string Query { get; set; } = string.Empty;
    public SearchType Type { get; set; }
    public int SearchCount { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public int Rank { get; set; }
}
