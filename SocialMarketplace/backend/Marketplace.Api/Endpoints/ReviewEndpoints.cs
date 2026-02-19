using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Marketplace.Slices.ReviewSlice;

namespace Marketplace.Api.Endpoints;

public static class ReviewEndpoints
{
    public static void MapReviewEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reviews").WithTags("Reviews");

        group.MapGet("/{id:guid}", async (Guid id, IReviewService reviewService) =>
        {
            var review = await reviewService.GetByIdAsync(id);
            return review != null ? Results.Ok(review) : Results.NotFound();
        })
        .WithName("GetReviewById");

        group.MapGet("/entity/{entityType}/{entityId:guid}", async (string entityType, Guid entityId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            IReviewService reviewService) =>
        {
            var (reviews, totalCount) = await reviewService.GetByEntityAsync(entityType, entityId, page, pageSize);
            return Results.Ok(new
            {
                data = reviews,
                pagination = new { page, pageSize, totalCount, totalPages = (int)Math.Ceiling(totalCount / (double)pageSize) }
            });
        })
        .WithName("GetEntityReviews");

        group.MapGet("/stats/{entityType}/{entityId:guid}", async (string entityType, Guid entityId, IReviewService reviewService) =>
        {
            var stats = await reviewService.GetStatsAsync(entityType, entityId);
            return Results.Ok(stats);
        })
        .WithName("GetReviewStats");

        group.MapPost("/", async (HttpContext context, [FromBody] CreateReviewDto dto, IReviewService reviewService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var id = await reviewService.CreateAsync(dto, userId.Value);
            return Results.Created($"/api/reviews/{id}", new { id });
        })
        .RequireAuthorization()
        .WithName("CreateReview");

        group.MapPost("/{id:guid}/respond", async (HttpContext context, Guid id,
            [FromBody] ReviewResponseRequest request, IReviewService reviewService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var responseId = await reviewService.RespondAsync(id, userId.Value, request.Content);
            return Results.Created($"/api/reviews/{id}/response", new { id = responseId });
        })
        .RequireAuthorization()
        .WithName("RespondToReview");

        group.MapDelete("/{id:guid}", async (HttpContext context, Guid id, IReviewService reviewService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var result = await reviewService.DeleteAsync(id, userId.Value);
            return result ? Results.NoContent() : Results.NotFound();
        })
        .RequireAuthorization()
        .WithName("DeleteReview");
    }

    private static Guid? GetUserId(HttpContext context)
    {
        var claim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && Guid.TryParse(claim, out var id) ? id : null;
    }
}

public record ReviewResponseRequest(string Content);
