using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Marketplace.Slices.NotificationSlice;

namespace Marketplace.Api.Endpoints;

public static class NotificationEndpoints
{
    public static void MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/notifications").WithTags("Notifications").RequireAuthorization();

        group.MapGet("/", async (HttpContext context,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool? unreadOnly = null,
            INotificationService notificationService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var (notifications, totalCount) = await notificationService.GetMyNotificationsAsync(userId.Value, page, pageSize, unreadOnly);
            return Results.Ok(new
            {
                data = notifications,
                pagination = new { page, pageSize, totalCount, totalPages = (int)Math.Ceiling(totalCount / (double)pageSize) }
            });
        })
        .WithName("GetNotifications");

        group.MapGet("/unread-count", async (HttpContext context, INotificationService notificationService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var count = await notificationService.GetUnreadCountAsync(userId.Value);
            return Results.Ok(new { count });
        })
        .WithName("GetUnreadCount");

        group.MapPost("/{id:guid}/read", async (HttpContext context, Guid id, INotificationService notificationService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            await notificationService.MarkAsReadAsync(id, userId.Value);
            return Results.Ok();
        })
        .WithName("MarkNotificationRead");

        group.MapPost("/read-all", async (HttpContext context, INotificationService notificationService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var count = await notificationService.MarkAllAsReadAsync(userId.Value);
            return Results.Ok(new { marked = count });
        })
        .WithName("MarkAllRead");
    }

    private static Guid? GetUserId(HttpContext context)
    {
        var claim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && Guid.TryParse(claim, out var id) ? id : null;
    }
}
