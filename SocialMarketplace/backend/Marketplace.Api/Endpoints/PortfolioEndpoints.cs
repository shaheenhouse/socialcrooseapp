using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Marketplace.Slices.PortfolioSlice;

namespace Marketplace.Api.Endpoints;

public static class PortfolioEndpoints
{
    public static void MapPortfolioEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/portfolios").WithTags("Portfolios");

        group.MapGet("/public", async (
            IPortfolioService portfolioService,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20) =>
        {
            var (portfolios, totalCount) = await portfolioService.GetPublicAsync(page, pageSize);
            return Results.Ok(new
            {
                data = portfolios,
                pagination = new { page, pageSize, totalCount, totalPages = (int)Math.Ceiling(totalCount / (double)pageSize) }
            });
        })
        .WithName("GetPublicPortfolios");

        group.MapGet("/slug/{slug}", async (string slug, IPortfolioService portfolioService) =>
        {
            var portfolio = await portfolioService.GetBySlugAsync(slug);
            if (portfolio == null) return Results.NotFound();
            if (!portfolio.IsPublic) return Results.NotFound();
            return Results.Ok(portfolio);
        })
        .WithName("GetPortfolioBySlug");

        group.MapGet("/me", async (HttpContext context, IPortfolioService portfolioService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var portfolio = await portfolioService.GetMyPortfolioAsync(userId.Value);
            return portfolio != null ? Results.Ok(portfolio) : Results.NotFound();
        })
        .RequireAuthorization()
        .WithName("GetMyPortfolio");

        group.MapGet("/{id:guid}", async (Guid id, IPortfolioService portfolioService) =>
        {
            var portfolio = await portfolioService.GetByIdAsync(id);
            return portfolio != null ? Results.Ok(portfolio) : Results.NotFound();
        })
        .WithName("GetPortfolioById");

        group.MapPost("/", async (HttpContext context, [FromBody] CreatePortfolioDto dto, IPortfolioService portfolioService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            try
            {
                var id = await portfolioService.CreateAsync(dto, userId.Value);
                return Results.Created($"/api/portfolios/{id}", new { id });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .RequireAuthorization()
        .WithName("CreatePortfolio");

        group.MapPatch("/{id:guid}", async (HttpContext context, Guid id, [FromBody] UpdatePortfolioDto dto, IPortfolioService portfolioService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var result = await portfolioService.UpdateAsync(id, userId.Value, dto);
            return result ? Results.Ok(new { message = "Portfolio updated" }) : Results.NotFound();
        })
        .RequireAuthorization()
        .WithName("UpdatePortfolio");

        group.MapDelete("/{id:guid}", async (HttpContext context, Guid id, IPortfolioService portfolioService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var result = await portfolioService.DeleteAsync(id, userId.Value);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .RequireAuthorization()
        .WithName("DeletePortfolio");
    }

    private static Guid? GetUserId(HttpContext context)
    {
        var claim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && Guid.TryParse(claim, out var id) ? id : null;
    }
}
