using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Marketplace.Slices.Social.Messaging;

namespace Marketplace.Api.Endpoints;

public static class MessageEndpoints
{
    public static void MapMessageEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/messages").WithTags("Messages").RequireAuthorization();

        group.MapGet("/conversations", async (HttpContext context,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] bool includeArchived = false,
            IMessageService messageService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var conversations = await messageService.GetConversationsAsync(userId.Value, page, pageSize, includeArchived);
            return Results.Ok(new { data = conversations });
        })
        .WithName("GetConversations");

        group.MapGet("/conversations/{id:guid}", async (HttpContext context, Guid id, IMessageService messageService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var conversation = await messageService.GetConversationAsync(id, userId.Value);
            return conversation != null ? Results.Ok(conversation) : Results.NotFound();
        })
        .WithName("GetConversation");

        group.MapGet("/unread-count", async (HttpContext context, IMessageService messageService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var count = await messageService.GetUnreadCountAsync(userId.Value);
            return Results.Ok(new { count });
        })
        .WithName("GetMessageUnreadCount");

        group.MapPost("/conversations/direct/{otherUserId:guid}", async (HttpContext context, Guid otherUserId, IMessageService messageService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var conversation = await messageService.GetOrCreateDirectConversationAsync(userId.Value, otherUserId);
            return Results.Ok(conversation);
        })
        .WithName("GetOrCreateDirectConversation");

        group.MapPost("/conversations/group", async (HttpContext context,
            [FromBody] CreateGroupRequest request, IMessageService messageService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var conversation = await messageService.CreateGroupConversationAsync(userId.Value, request.Title, request.ParticipantIds);
            return Results.Created($"/api/messages/conversations/{conversation.Id}", conversation);
        })
        .WithName("CreateGroupConversation");

        group.MapGet("/conversations/{id:guid}/messages", async (HttpContext context, Guid id,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] Guid? before = null,
            IMessageService messageService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var messages = await messageService.GetMessagesAsync(id, userId.Value, page, pageSize, before);
            return Results.Ok(new { data = messages });
        })
        .WithName("GetConversationMessages");

        group.MapPost("/conversations/{id:guid}/messages", async (HttpContext context, Guid id,
            [FromBody] SendMessageRequest request, IMessageService messageService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var message = await messageService.SendMessageAsync(id, userId.Value, request);
            return Results.Created($"/api/messages/{message.Id}", message);
        })
        .WithName("SendMessage");

        group.MapPatch("/{messageId:guid}", async (HttpContext context, Guid messageId,
            [FromBody] EditMessageRequest request, IMessageService messageService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            var message = await messageService.EditMessageAsync(messageId, userId.Value, request.Content);
            return Results.Ok(message);
        })
        .WithName("EditMessage");

        group.MapDelete("/{messageId:guid}", async (HttpContext context, Guid messageId, IMessageService messageService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            await messageService.DeleteMessageAsync(messageId, userId.Value);
            return Results.NoContent();
        })
        .WithName("DeleteMessage");

        group.MapPost("/conversations/{id:guid}/read", async (HttpContext context, Guid id, IMessageService messageService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            await messageService.MarkAsReadAsync(id, userId.Value);
            return Results.Ok();
        })
        .WithName("MarkConversationRead");

        group.MapPatch("/conversations/{id:guid}/mute", async (HttpContext context, Guid id,
            [FromBody] MuteRequest request, IMessageService messageService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            await messageService.MuteConversationAsync(id, userId.Value, request.Muted);
            return Results.Ok();
        })
        .WithName("MuteConversation");

        group.MapPatch("/conversations/{id:guid}/archive", async (HttpContext context, Guid id,
            [FromBody] ArchiveRequest request, IMessageService messageService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            await messageService.ArchiveConversationAsync(id, userId.Value, request.Archived);
            return Results.Ok();
        })
        .WithName("ArchiveConversation");

        group.MapPost("/conversations/{id:guid}/leave", async (HttpContext context, Guid id, IMessageService messageService) =>
        {
            var userId = GetUserId(context);
            if (userId == null) return Results.Unauthorized();
            await messageService.LeaveConversationAsync(id, userId.Value);
            return Results.NoContent();
        })
        .WithName("LeaveConversation");
    }

    private static Guid? GetUserId(HttpContext context)
    {
        var claim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null && Guid.TryParse(claim, out var id) ? id : null;
    }
}

public record CreateGroupRequest(string Title, IEnumerable<Guid> ParticipantIds);
public record EditMessageRequest(string Content);
public record MuteRequest(bool Muted);
public record ArchiveRequest(bool Archived);
