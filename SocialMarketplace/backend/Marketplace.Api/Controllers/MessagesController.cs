using Marketplace.Slices.Social.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Marketplace.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;

    public MessagesController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>
    /// Get all conversations
    /// </summary>
    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool includeArchived = false)
    {
        var conversations = await _messageService.GetConversationsAsync(GetUserId(), page, pageSize, includeArchived);
        return Ok(conversations);
    }

    /// <summary>
    /// Get a specific conversation
    /// </summary>
    [HttpGet("conversations/{conversationId:guid}")]
    public async Task<IActionResult> GetConversation(Guid conversationId)
    {
        var conversation = await _messageService.GetConversationAsync(conversationId, GetUserId());
        if (conversation == null) return NotFound();
        return Ok(conversation);
    }

    /// <summary>
    /// Get unread message count
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var count = await _messageService.GetUnreadCountAsync(GetUserId());
        return Ok(new { UnreadCount = count });
    }

    /// <summary>
    /// Start or get a direct conversation with a user
    /// </summary>
    [HttpPost("conversations/direct/{userId:guid}")]
    public async Task<IActionResult> GetOrCreateDirectConversation(Guid userId)
    {
        var conversation = await _messageService.GetOrCreateDirectConversationAsync(GetUserId(), userId);
        return Ok(conversation);
    }

    /// <summary>
    /// Create a group conversation
    /// </summary>
    [HttpPost("conversations/group")]
    public async Task<IActionResult> CreateGroupConversation([FromBody] CreateGroupConversationRequest request)
    {
        var conversation = await _messageService.CreateGroupConversationAsync(
            GetUserId(), request.Title, request.ParticipantIds);
        return Ok(conversation);
    }

    /// <summary>
    /// Get messages in a conversation
    /// </summary>
    [HttpGet("conversations/{conversationId:guid}/messages")]
    public async Task<IActionResult> GetMessages(
        Guid conversationId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] Guid? beforeMessageId = null)
    {
        try
        {
            var messages = await _messageService.GetMessagesAsync(
                conversationId, GetUserId(), page, pageSize, beforeMessageId);
            return Ok(messages);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Send a message
    /// </summary>
    [HttpPost("conversations/{conversationId:guid}/messages")]
    public async Task<IActionResult> SendMessage(Guid conversationId, [FromBody] SendMessageRequest request)
    {
        try
        {
            var message = await _messageService.SendMessageAsync(conversationId, GetUserId(), request);
            return Ok(message);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Edit a message
    /// </summary>
    [HttpPatch("{messageId:guid}")]
    public async Task<IActionResult> EditMessage(Guid messageId, [FromBody] EditMessageRequest request)
    {
        try
        {
            var message = await _messageService.EditMessageAsync(messageId, GetUserId(), request.Content);
            return Ok(message);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Delete a message
    /// </summary>
    [HttpDelete("{messageId:guid}")]
    public async Task<IActionResult> DeleteMessage(Guid messageId)
    {
        try
        {
            await _messageService.DeleteMessageAsync(messageId, GetUserId());
            return Ok();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Mark conversation as read
    /// </summary>
    [HttpPost("conversations/{conversationId:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid conversationId)
    {
        await _messageService.MarkAsReadAsync(conversationId, GetUserId());
        return Ok();
    }

    /// <summary>
    /// Mute/unmute a conversation
    /// </summary>
    [HttpPatch("conversations/{conversationId:guid}/mute")]
    public async Task<IActionResult> MuteConversation(Guid conversationId, [FromBody] MuteRequest request)
    {
        try
        {
            await _messageService.MuteConversationAsync(conversationId, GetUserId(), request.Muted);
            return Ok();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Archive/unarchive a conversation
    /// </summary>
    [HttpPatch("conversations/{conversationId:guid}/archive")]
    public async Task<IActionResult> ArchiveConversation(Guid conversationId, [FromBody] ArchiveRequest request)
    {
        try
        {
            await _messageService.ArchiveConversationAsync(conversationId, GetUserId(), request.Archived);
            return Ok();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Leave a group conversation
    /// </summary>
    [HttpPost("conversations/{conversationId:guid}/leave")]
    public async Task<IActionResult> LeaveConversation(Guid conversationId)
    {
        await _messageService.LeaveConversationAsync(conversationId, GetUserId());
        return Ok();
    }
}

public record CreateGroupConversationRequest
{
    public string Title { get; init; } = string.Empty;
    public List<Guid> ParticipantIds { get; init; } = [];
}

public record EditMessageRequest
{
    public string Content { get; init; } = string.Empty;
}

public record MuteRequest
{
    public bool Muted { get; init; }
}

public record ArchiveRequest
{
    public bool Archived { get; init; }
}
