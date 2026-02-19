using Marketplace.Database.Entities.Social;
using Marketplace.Slices.Social.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return claim != null ? Guid.Parse(claim) : null;
    }

    /// <summary>
    /// Unified search across all types
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> SearchAll(
        [FromQuery] string q,
        [FromQuery] string? location,
        [FromQuery] string? industry,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { Error = "Search query is required" });

        var filters = new SearchFilters
        {
            Location = location,
            Industry = industry,
            MinPrice = minPrice,
            MaxPrice = maxPrice
        };

        var results = await _searchService.SearchAllAsync(q, filters, GetUserId(), page, pageSize);
        return Ok(results);
    }

    /// <summary>
    /// Search users/people
    /// </summary>
    [HttpGet("users")]
    public async Task<IActionResult> SearchUsers(
        [FromQuery] string q,
        [FromQuery] string? location,
        [FromQuery] string? skills,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { Error = "Search query is required" });

        var filters = new SearchFilters
        {
            Location = location,
            Skills = skills?.Split(',').Select(s => s.Trim())
        };

        var results = await _searchService.SearchUsersAsync(q, filters, GetUserId(), page, pageSize);
        return Ok(results);
    }

    /// <summary>
    /// Search services
    /// </summary>
    [HttpGet("services")]
    public async Task<IActionResult> SearchServices(
        [FromQuery] string q,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] Guid? categoryId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { Error = "Search query is required" });

        var filters = new SearchFilters
        {
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            CategoryId = categoryId
        };

        var results = await _searchService.SearchServicesAsync(q, filters, GetUserId(), page, pageSize);
        return Ok(results);
    }

    /// <summary>
    /// Search projects
    /// </summary>
    [HttpGet("projects")]
    public async Task<IActionResult> SearchProjects(
        [FromQuery] string q,
        [FromQuery] decimal? minBudget,
        [FromQuery] decimal? maxBudget,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { Error = "Search query is required" });

        var filters = new SearchFilters
        {
            MinPrice = minBudget,
            MaxPrice = maxBudget,
            Status = status
        };

        var results = await _searchService.SearchProjectsAsync(q, filters, GetUserId(), page, pageSize);
        return Ok(results);
    }

    /// <summary>
    /// Search companies/organizations
    /// </summary>
    [HttpGet("companies")]
    public async Task<IActionResult> SearchCompanies(
        [FromQuery] string q,
        [FromQuery] string? industry,
        [FromQuery] string? location,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { Error = "Search query is required" });

        var filters = new SearchFilters
        {
            Industry = industry,
            Location = location
        };

        var results = await _searchService.SearchCompaniesAsync(q, filters, GetUserId(), page, pageSize);
        return Ok(results);
    }

    /// <summary>
    /// Search posts
    /// </summary>
    [HttpGet("posts")]
    public async Task<IActionResult> SearchPosts(
        [FromQuery] string q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { Error = "Search query is required" });

        var results = await _searchService.SearchPostsAsync(q, new SearchFilters(), GetUserId(), page, pageSize);
        return Ok(results);
    }

    /// <summary>
    /// Get recent searches
    /// </summary>
    [HttpGet("history")]
    [Authorize]
    public async Task<IActionResult> GetSearchHistory()
    {
        var history = await _searchService.GetRecentSearchesAsync(GetUserId()!.Value);
        return Ok(history);
    }

    /// <summary>
    /// Clear search history
    /// </summary>
    [HttpDelete("history")]
    [Authorize]
    public async Task<IActionResult> ClearSearchHistory([FromQuery] Guid? searchId)
    {
        await _searchService.ClearSearchHistoryAsync(GetUserId()!.Value, searchId);
        return Ok();
    }

    /// <summary>
    /// Get trending searches
    /// </summary>
    [HttpGet("trending")]
    public async Task<IActionResult> GetTrendingSearches([FromQuery] SearchType? type)
    {
        var trending = await _searchService.GetTrendingSearchesAsync(type);
        return Ok(trending);
    }

    /// <summary>
    /// Record a search result click
    /// </summary>
    [HttpPost("click")]
    [Authorize]
    public async Task<IActionResult> RecordClick([FromBody] RecordClickRequest request)
    {
        await _searchService.RecordSearchClickAsync(GetUserId()!.Value, request.SearchId, request.ResultId);
        return Ok();
    }
}

public record RecordClickRequest
{
    public Guid SearchId { get; init; }
    public Guid ResultId { get; init; }
}
