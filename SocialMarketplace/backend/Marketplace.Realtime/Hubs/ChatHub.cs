using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Marketplace.Realtime.Hubs;

/// <summary>
/// SignalR hub for real-time messaging
/// </summary>
[Authorize]
public class ChatHub : Hub
{
    private readonly ILogger<ChatHub> _logger;
    private static readonly Dictionary<string, HashSet<string>> _userConnections = new();
    private static readonly object _lock = new();

    public ChatHub(ILogger<ChatHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId != null)
        {
            lock (_lock)
            {
                if (!_userConnections.ContainsKey(userId))
                {
                    _userConnections[userId] = new HashSet<string>();
                }
                _userConnections[userId].Add(Context.ConnectionId);
            }

            // Add to user group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");
            
            // Notify user's contacts they're online
            await Clients.Others.SendAsync("UserOnline", userId);
            
            _logger.LogInformation("User {UserId} connected to chat hub", userId);
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId != null)
        {
            bool isLastConnection = false;
            lock (_lock)
            {
                if (_userConnections.TryGetValue(userId, out var connections))
                {
                    connections.Remove(Context.ConnectionId);
                    if (connections.Count == 0)
                    {
                        _userConnections.Remove(userId);
                        isLastConnection = true;
                    }
                }
            }

            if (isLastConnection)
            {
                // Notify user's contacts they're offline
                await Clients.Others.SendAsync("UserOffline", userId);
            }

            _logger.LogInformation("User {UserId} disconnected from chat hub", userId);
        }
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Join a chat room
    /// </summary>
    public async Task JoinRoom(Guid roomId)
    {
        var userId = GetUserId();
        await Groups.AddToGroupAsync(Context.ConnectionId, $"room:{roomId}");
        _logger.LogInformation("User {UserId} joined room {RoomId}", userId, roomId);
        
        await Clients.Group($"room:{roomId}").SendAsync("UserJoinedRoom", userId, roomId);
    }

    /// <summary>
    /// Leave a chat room
    /// </summary>
    public async Task LeaveRoom(Guid roomId)
    {
        var userId = GetUserId();
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"room:{roomId}");
        _logger.LogInformation("User {UserId} left room {RoomId}", userId, roomId);
        
        await Clients.Group($"room:{roomId}").SendAsync("UserLeftRoom", userId, roomId);
    }

    /// <summary>
    /// Send a message to a room
    /// </summary>
    public async Task SendMessage(Guid roomId, string content, string? attachmentUrl = null)
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(content) && string.IsNullOrWhiteSpace(attachmentUrl))
        {
            return;
        }

        var message = new
        {
            Id = Guid.NewGuid(),
            RoomId = roomId,
            SenderId = userId,
            Content = content,
            AttachmentUrl = attachmentUrl,
            Timestamp = DateTime.UtcNow,
            Status = "sent"
        };

        // In production, save to database first
        _logger.LogInformation("User {UserId} sent message to room {RoomId}", userId, roomId);
        
        await Clients.Group($"room:{roomId}").SendAsync("NewMessage", message);
    }

    /// <summary>
    /// Send a direct message
    /// </summary>
    public async Task SendDirectMessage(Guid recipientId, string content, string? attachmentUrl = null)
    {
        var senderId = GetUserId();
        if (string.IsNullOrWhiteSpace(content) && string.IsNullOrWhiteSpace(attachmentUrl))
        {
            return;
        }

        var message = new
        {
            Id = Guid.NewGuid(),
            SenderId = senderId,
            RecipientId = recipientId,
            Content = content,
            AttachmentUrl = attachmentUrl,
            Timestamp = DateTime.UtcNow,
            Status = "sent"
        };

        // In production, save to database first
        _logger.LogInformation("User {SenderId} sent direct message to {RecipientId}", senderId, recipientId);
        
        // Send to recipient
        await Clients.Group($"user:{recipientId}").SendAsync("NewDirectMessage", message);
        // Send back to sender for confirmation
        await Clients.Caller.SendAsync("MessageSent", message);
    }

    /// <summary>
    /// Indicate typing status
    /// </summary>
    public async Task Typing(Guid roomId)
    {
        var userId = GetUserId();
        await Clients.OthersInGroup($"room:{roomId}").SendAsync("UserTyping", userId, roomId);
    }

    /// <summary>
    /// Indicate stopped typing
    /// </summary>
    public async Task StopTyping(Guid roomId)
    {
        var userId = GetUserId();
        await Clients.OthersInGroup($"room:{roomId}").SendAsync("UserStoppedTyping", userId, roomId);
    }

    /// <summary>
    /// Mark messages as read
    /// </summary>
    public async Task MarkAsRead(Guid roomId, Guid lastReadMessageId)
    {
        var userId = GetUserId();
        _logger.LogInformation("User {UserId} marked messages as read up to {MessageId} in room {RoomId}", 
            userId, lastReadMessageId, roomId);
        
        await Clients.OthersInGroup($"room:{roomId}").SendAsync("MessagesRead", userId, roomId, lastReadMessageId);
    }

    /// <summary>
    /// Get online status of users
    /// </summary>
    public async Task GetOnlineUsers(Guid[] userIds)
    {
        var onlineUsers = new List<string>();
        lock (_lock)
        {
            foreach (var userId in userIds)
            {
                if (_userConnections.ContainsKey(userId.ToString()))
                {
                    onlineUsers.Add(userId.ToString());
                }
            }
        }
        await Clients.Caller.SendAsync("OnlineUsers", onlineUsers);
    }

    private string? GetUserId()
    {
        return Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}
