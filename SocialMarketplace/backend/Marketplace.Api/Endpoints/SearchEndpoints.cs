using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Marketplace.Slices.Social.Search;
using Marketplace.Database.Entities.Social;

namespace Marketplace.Api.Endpoints;

public static class SearchEndpoints
{
    public static void MapSearchEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/search").WithTags("Search");

        group.MapGet("/", async (HttpContext context,
            ISearchService searchService,
            [FromQuery] string q = "",
            [FromQuery] string? location = null,
            [FromQuery] string? industry = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
        {
            var userId = GetUserId(context);
            var filters = new SearchFilters
            {
                Location = location, Industry = industry,
                MinPrice = minPrice, MaxPrice = maxPrice
            };
            var results = await searchService.SearchAllAsync(q, filters, userId, page, pageSize);
            return Results.Ok(results);
        })
        .WithName("UnifiedSearch");

        group.MapGet("/users", async (HttpContext context,
            ISearchService searchService,
            [FromQuery] string q = "",
            [FromQuery] string? location = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
        {
            var userId = GetUserId(context);
            var filters = new SearchFilters { Location = location };
            var results = await searchService.SearchUsersAsync(q, filters, userId, page, pageSize);
            return Results.Ok(new { data = results });
        })
        .WithName("SearchUsers");

        group.MapGet("/services", async (HttpContext context,
            ISearchService searchService,
            [FromQuery] string q = "",
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
        {
            var userId = GetUserId(context);
            var filters = new SearchFilters { MinPrice = minPrice, MaxPrice = maxPrice };
            var results = await searchService.SearchServicesAsync(q, filters, userId, page, pageSize);
            return Results.Ok(new { data = results });
        })
        .WithName("SearchServices");

        group.MapGet("/projects", async (HttpContext context,
            ISearchService searchService,
            [FromQuery] string q = "",
            [FromQuery] string? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
        {
            var userId = GetUserId(context);
            var filters = new SearchFilters { Status = status };
            var results = await searchService.SearchProjectsAsync(q, filters, userId, page, pageSize);
            return Results.Ok(new { data = results });
        })
        .WithName("SearchProjects");

        group.MapGet("/companies", async (HttpContext context,
            ISearchService searchService,
            [FromQuery] string q = "",
            [FromQuery] string? industry = null,
            [FromQuery] string? location = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
        {
            var userId = GetUserId(context);
            var filters = new SearchFilters { Industry = industry, Location = location };
            var results = await searchService.SearchCompaniesAsync(q, filters, userId, page, pageSize);
            return Results.Ok(new { data = results });
        })
        .WithName("SearchCompanies");

        group.MapGet("/posts", async (HttpContext context,
            ISearchService searchService,
            [FromQuery] string q = "",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
        {
            var userId = GetUserId(context);
            var results = await searchService.SearchPostsAsync(q, new SearchFilters(), userId, page, pageSize);
            return Results.Ok(new { data = results });
        })
        .WithName("SearchPosts");

        group.MapGet("/history", async (HttpContext context, ISearchService searchService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var history = await searchService.GetRecentSearchesAsync(userId.Value);
            return Results.Ok(new { data = history });
        })
        .RequireAuthorization()
        .WithName("GetSearchHistory");

        group.MapDelete("/history", async (HttpContext context,
            ISearchService searchService,
            [FromQuery] Guid? id = null) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            await searchService.ClearSearchHistoryAsync(userId.Value, id);
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("ClearSearchHistory");

        group.MapGet("/trending", async ([FromQuery] string? type, ISearchService searchService) =>
        {
            SearchType? searchType = type != null && Enum.TryParse<SearchType>(type, true, out var t) ? t : null;
            var trending = await searchService.GetTrendingSearchesAsync(searchType);
            return Results.Ok(new { data = trending });
        })
        .WithName("GetTrendingSearches");
    }

    private static Guid? GetUserId(HttpContext context)
    {
        var claim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && Guid.TryParse(claim, out var id) ? id : null;
    }
}
