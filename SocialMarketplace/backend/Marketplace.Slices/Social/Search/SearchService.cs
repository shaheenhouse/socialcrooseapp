using Marketplace.Database.Entities.Social;
using System.Text.Json;

namespace Marketplace.Slices.Social.Search;

public interface ISearchService
{
    Task<UnifiedSearchResult> SearchAllAsync(string query, SearchFilters filters, Guid? userId, int page, int pageSize);
    Task<IEnumerable<UserSearchResult>> SearchUsersAsync(string query, SearchFilters filters, Guid? userId, int page, int pageSize);
    Task<IEnumerable<ServiceSearchResult>> SearchServicesAsync(string query, SearchFilters filters, Guid? userId, int page, int pageSize);
    Task<IEnumerable<ProjectSearchResult>> SearchProjectsAsync(string query, SearchFilters filters, Guid? userId, int page, int pageSize);
    Task<IEnumerable<CompanySearchResult>> SearchCompaniesAsync(string query, SearchFilters filters, Guid? userId, int page, int pageSize);
    Task<IEnumerable<PostSearchResult>> SearchPostsAsync(string query, SearchFilters filters, Guid? userId, int page, int pageSize);
    Task<IEnumerable<RecentSearchDto>> GetRecentSearchesAsync(Guid userId);
    Task ClearSearchHistoryAsync(Guid userId, Guid? searchId = null);
    Task<IEnumerable<TrendingSearchDto>> GetTrendingSearchesAsync(SearchType? type);
    Task RecordSearchClickAsync(Guid userId, Guid searchId, Guid resultId);
}

public class SearchService : ISearchService
{
    private readonly ISearchRepository _repository;

    public SearchService(ISearchRepository repository)
    {
        _repository = repository;
    }

    public async Task<UnifiedSearchResult> SearchAllAsync(string query, SearchFilters filters, Guid? userId, int page, int pageSize)
    {
        // Run all searches in parallel
        var usersTask = _repository.SearchUsersAsync(query, filters, page, 5);
        var servicesTask = _repository.SearchServicesAsync(query, filters, page, 5);
        var projectsTask = _repository.SearchProjectsAsync(query, filters, page, 5);
        var companiesTask = _repository.SearchCompaniesAsync(query, filters, page, 5);
        var postsTask = _repository.SearchPostsAsync(query, filters, page, 5);

        await Task.WhenAll(usersTask, servicesTask, projectsTask, companiesTask, postsTask);

        var result = new UnifiedSearchResult
        {
            Users = usersTask.Result,
            Services = servicesTask.Result,
            Projects = projectsTask.Result,
            Companies = companiesTask.Result,
            Posts = postsTask.Result
        };

        // Save search history
        if (userId.HasValue)
        {
            var totalResults = result.Users.Count() + result.Services.Count() + 
                              result.Projects.Count() + result.Companies.Count() + 
                              result.Posts.Count();
            
            await _repository.SaveSearchHistoryAsync(new SearchHistory
            {
                UserId = userId.Value,
                Query = query,
                Type = SearchType.All,
                Filters = filters != null ? JsonSerializer.Serialize(filters) : null,
                ResultCount = totalResults
            });
        }

        return result;
    }

    public async Task<IEnumerable<UserSearchResult>> SearchUsersAsync(string query, SearchFilters filters, Guid? userId, int page, int pageSize)
    {
        var results = await _repository.SearchUsersAsync(query, filters, page, pageSize);
        
        if (userId.HasValue)
        {
            await _repository.SaveSearchHistoryAsync(new SearchHistory
            {
                UserId = userId.Value,
                Query = query,
                Type = SearchType.People,
                Filters = filters != null ? JsonSerializer.Serialize(filters) : null,
                ResultCount = results.Count()
            });
        }

        return results;
    }

    public async Task<IEnumerable<ServiceSearchResult>> SearchServicesAsync(string query, SearchFilters filters, Guid? userId, int page, int pageSize)
    {
        var results = await _repository.SearchServicesAsync(query, filters, page, pageSize);
        
        if (userId.HasValue)
        {
            await _repository.SaveSearchHistoryAsync(new SearchHistory
            {
                UserId = userId.Value,
                Query = query,
                Type = SearchType.Services,
                Filters = filters != null ? JsonSerializer.Serialize(filters) : null,
                ResultCount = results.Count()
            });
        }

        return results;
    }

    public async Task<IEnumerable<ProjectSearchResult>> SearchProjectsAsync(string query, SearchFilters filters, Guid? userId, int page, int pageSize)
    {
        var results = await _repository.SearchProjectsAsync(query, filters, page, pageSize);
        
        if (userId.HasValue)
        {
            await _repository.SaveSearchHistoryAsync(new SearchHistory
            {
                UserId = userId.Value,
                Query = query,
                Type = SearchType.Projects,
                Filters = filters != null ? JsonSerializer.Serialize(filters) : null,
                ResultCount = results.Count()
            });
        }

        return results;
    }

    public async Task<IEnumerable<CompanySearchResult>> SearchCompaniesAsync(string query, SearchFilters filters, Guid? userId, int page, int pageSize)
    {
        var results = await _repository.SearchCompaniesAsync(query, filters, page, pageSize);
        
        if (userId.HasValue)
        {
            await _repository.SaveSearchHistoryAsync(new SearchHistory
            {
                UserId = userId.Value,
                Query = query,
                Type = SearchType.Companies,
                Filters = filters != null ? JsonSerializer.Serialize(filters) : null,
                ResultCount = results.Count()
            });
        }

        return results;
    }

    public async Task<IEnumerable<PostSearchResult>> SearchPostsAsync(string query, SearchFilters filters, Guid? userId, int page, int pageSize)
    {
        var results = await _repository.SearchPostsAsync(query, filters, page, pageSize);
        
        if (userId.HasValue)
        {
            await _repository.SaveSearchHistoryAsync(new SearchHistory
            {
                UserId = userId.Value,
                Query = query,
                Type = SearchType.Posts,
                Filters = filters != null ? JsonSerializer.Serialize(filters) : null,
                ResultCount = results.Count()
            });
        }

        return results;
    }

    public async Task<IEnumerable<RecentSearchDto>> GetRecentSearchesAsync(Guid userId)
    {
        var searches = await _repository.GetRecentSearchesAsync(userId, 10);
        return searches.Select(s => new RecentSearchDto
        {
            Id = s.Id,
            Query = s.Query,
            Type = s.Type,
            SearchedAt = s.CreatedAt
        });
    }

    public async Task ClearSearchHistoryAsync(Guid userId, Guid? searchId = null)
    {
        await _repository.DeleteSearchHistoryAsync(userId, searchId);
    }

    public async Task<IEnumerable<TrendingSearchDto>> GetTrendingSearchesAsync(SearchType? type)
    {
        var trending = await _repository.GetTrendingSearchesAsync(type, 10);
        return trending.Select(t => new TrendingSearchDto
        {
            Query = t.Query,
            Type = t.Type,
            SearchCount = t.SearchCount,
            Rank = t.Rank
        });
    }

    public async Task RecordSearchClickAsync(Guid userId, Guid searchId, Guid resultId)
    {
        // This would update the search history record with the clicked result
        // For now, we'll just save a new entry indicating the click
        await _repository.SaveSearchHistoryAsync(new SearchHistory
        {
            UserId = userId,
            Query = string.Empty, // Would need to look up original query
            Type = SearchType.All,
            ClickedResultId = resultId
        });
    }
}

public record UnifiedSearchResult
{
    public IEnumerable<UserSearchResult> Users { get; init; } = [];
    public IEnumerable<ServiceSearchResult> Services { get; init; } = [];
    public IEnumerable<ProjectSearchResult> Projects { get; init; } = [];
    public IEnumerable<CompanySearchResult> Companies { get; init; } = [];
    public IEnumerable<PostSearchResult> Posts { get; init; } = [];
}

public record RecentSearchDto
{
    public Guid Id { get; init; }
    public string Query { get; init; } = string.Empty;
    public SearchType Type { get; init; }
    public DateTime SearchedAt { get; init; }
}

public record TrendingSearchDto
{
    public string Query { get; init; } = string.Empty;
    public SearchType Type { get; init; }
    public int SearchCount { get; init; }
    public int Rank { get; init; }
}
