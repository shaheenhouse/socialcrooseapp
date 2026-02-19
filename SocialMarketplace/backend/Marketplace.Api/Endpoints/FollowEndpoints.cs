using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Marketplace.Slices.Social.Follows;
using Marketplace.Database.Entities.Social;

namespace Marketplace.Api.Endpoints;

public static class FollowEndpoints
{
    public static void MapFollowEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/follows").WithTags("Follows");

        group.MapGet("/stats", async (HttpContext context, IFollowService followService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var stats = await followService.GetFollowStatsAsync(userId.Value);
            return Results.Ok(stats);
        })
        .RequireAuthorization()
        .WithName("GetMyFollowStats");

        group.MapGet("/stats/{userId:guid}", async (Guid userId, IFollowService followService) =>
        {
            var stats = await followService.GetFollowStatsAsync(userId);
            return Results.Ok(stats);
        })
        .WithName("GetUserFollowStats");

        group.MapGet("/status", async (HttpContext context,
            [FromQuery] Guid targetId,
            [FromQuery] string targetType = "User",
            IFollowService followService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var type = Enum.TryParse<FollowTargetType>(targetType, true, out var t) ? t : FollowTargetType.User;
            var status = await followService.GetFollowStatusAsync(userId.Value, targetId, type);
            return Results.Ok(status);
        })
        .RequireAuthorization()
        .WithName("GetFollowStatus");

        group.MapGet("/followers", async (HttpContext context,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            IFollowService followService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var followers = await followService.GetFollowersAsync(userId.Value, FollowTargetType.User, page, pageSize);
            return Results.Ok(new { data = followers });
        })
        .RequireAuthorization()
        .WithName("GetMyFollowers");

        group.MapGet("/following", async (HttpContext context,
            [FromQuery] string? targetType = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            IFollowService followService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var type = targetType != null && Enum.TryParse<FollowTargetType>(targetType, true, out var t) ? (FollowTargetType?)t : null;
            var following = await followService.GetFollowingAsync(userId.Value, type, page, pageSize);
            return Results.Ok(new { data = following });
        })
        .RequireAuthorization()
        .WithName("GetMyFollowing");

        group.MapPost("/", async (HttpContext context,
            [FromBody] FollowRequest request, IFollowService followService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var type = Enum.TryParse<FollowTargetType>(request.TargetType, true, out var t) ? t : FollowTargetType.User;
            var id = await followService.FollowAsync(userId.Value, request.TargetId, type);
            return Results.Created($"/api/follows/{id}", new { id });
        })
        .RequireAuthorization()
        .WithName("Follow");

        group.MapDelete("/", async (HttpContext context,
            [FromQuery] Guid targetId,
            [FromQuery] string targetType = "User",
            IFollowService followService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var type = Enum.TryParse<FollowTargetType>(targetType, true, out var t) ? t : FollowTargetType.User;
            await followService.UnfollowAsync(userId.Value, targetId, type);
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithName("Unfollow");

        group.MapPatch("/notifications", async (HttpContext context,
            [FromBody] ToggleNotificationsRequest request, IFollowService followService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var type = Enum.TryParse<FollowTargetType>(request.TargetType, true, out var t) ? t : FollowTargetType.User;
            await followService.ToggleNotificationsAsync(userId.Value, request.TargetId, type, request.Enabled);
            return Results.Ok();
        })
        .RequireAuthorization()
        .WithName("ToggleFollowNotifications");
    }

    private static Guid? GetUserId(HttpContext context)
    {
        var claim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && Guid.TryParse(claim, out var id) ? id : null;
    }
}

public record FollowRequest(Guid TargetId, string TargetType = "User");
public record ToggleNotificationsRequest(Guid TargetId, string TargetType, bool Enabled);
