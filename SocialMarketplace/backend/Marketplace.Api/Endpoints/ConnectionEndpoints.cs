using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Marketplace.Slices.Social.Connections;

namespace Marketplace.Api.Endpoints;

public static class ConnectionEndpoints
{
    public static void MapConnectionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/connections").WithTags("Connections").RequireAuthorization();

        group.MapGet("/", async (HttpContext context,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            IConnectionService connectionService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var result = await connectionService.GetMyConnectionsAsync(userId.Value, null, page, pageSize);
            return Results.Ok(new
            {
                data = result.Items,
                pagination = new { page, pageSize, totalCount = result.TotalCount, totalPages = result.TotalPages }
            });
        })
        .WithName("GetMyConnections");

        group.MapGet("/pending", async (HttpContext context,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            IConnectionService connectionService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var result = await connectionService.GetPendingRequestsAsync(userId.Value, page, pageSize);
            return Results.Ok(new { data = result.Items });
        })
        .WithName("GetPendingConnections");

        group.MapGet("/sent", async (HttpContext context,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            IConnectionService connectionService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var result = await connectionService.GetSentRequestsAsync(userId.Value, page, pageSize);
            return Results.Ok(new { data = result.Items });
        })
        .WithName("GetSentConnections");

        group.MapGet("/stats", async (HttpContext context, IConnectionService connectionService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var stats = await connectionService.GetConnectionStatsAsync(userId.Value);
            return Results.Ok(stats);
        })
        .WithName("GetConnectionStats");

        group.MapGet("/suggestions", async (HttpContext context,
            [FromQuery] int limit = 20,
            IConnectionService connectionService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var suggestions = await connectionService.GetSuggestionsAsync(userId.Value, limit);
            return Results.Ok(new { data = suggestions });
        })
        .WithName("GetConnectionSuggestions");

        group.MapGet("/status/{otherUserId:guid}", async (HttpContext context, Guid otherUserId, IConnectionService connectionService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var status = await connectionService.GetConnectionStatusAsync(userId.Value, otherUserId);
            return Results.Ok(status);
        })
        .WithName("GetConnectionStatus");

        group.MapPost("/request", async (HttpContext context,
            [FromBody] ConnectionRequest request, IConnectionService connectionService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            try
            {
                var id = await connectionService.SendConnectionRequestAsync(userId.Value, request.UserId, request.Message);
                return Results.Created($"/api/connections/{id}", new { id });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("SendConnectionRequest");

        group.MapPost("/{id:guid}/accept", async (HttpContext context, Guid id, IConnectionService connectionService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            await connectionService.AcceptConnectionAsync(id, userId.Value);
            return Results.Ok(new { message = "Connection accepted" });
        })
        .WithName("AcceptConnection");

        group.MapPost("/{id:guid}/reject", async (HttpContext context, Guid id, IConnectionService connectionService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            await connectionService.RejectConnectionAsync(id, userId.Value);
            return Results.Ok(new { message = "Connection rejected" });
        })
        .WithName("RejectConnection");

        group.MapPost("/{id:guid}/withdraw", async (HttpContext context, Guid id, IConnectionService connectionService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            await connectionService.WithdrawConnectionAsync(id, userId.Value);
            return Results.Ok(new { message = "Request withdrawn" });
        })
        .WithName("WithdrawConnection");

        group.MapDelete("/{id:guid}", async (HttpContext context, Guid id, IConnectionService connectionService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            await connectionService.RemoveConnectionAsync(id, userId.Value);
            return Results.NoContent();
        })
        .WithName("RemoveConnection");

        group.MapPost("/block/{blockedUserId:guid}", async (HttpContext context, Guid blockedUserId, IConnectionService connectionService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            await connectionService.BlockUserAsync(userId.Value, blockedUserId);
            return Results.Ok(new { message = "User blocked" });
        })
        .WithName("BlockUser");
    }

    private static Guid? GetUserId(HttpContext context)
    {
        var claim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && Guid.TryParse(claim, out var id) ? id : null;
    }
}

public record ConnectionRequest(Guid UserId, string? Message = null);
