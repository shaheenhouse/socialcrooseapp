using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Collections.Concurrent;

namespace Marketplace.Realtime.Hubs;

/// <summary>
/// SignalR hub for user presence/online status
/// </summary>
[Authorize]
public class PresenceHub : Hub
{
    private readonly ILogger<PresenceHub> _logger;
    private static readonly ConcurrentDictionary<string, UserPresence> _onlineUsers = new();

    public PresenceHub(ILogger<PresenceHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId != null)
        {
            var presence = new UserPresence
            {
                UserId = userId,
                ConnectionId = Context.ConnectionId,
                ConnectedAt = DateTime.UtcNow,
                Status = "online"
            };

            _onlineUsers.AddOrUpdate(userId, presence, (key, existing) =>
            {
                existing.ConnectionId = Context.ConnectionId;
                existing.ConnectedAt = DateTime.UtcNow;
                existing.Status = "online";
                return existing;
            });

            // Notify all clients about the new online user
            await Clients.All.SendAsync("UserConnected", new
            {
                UserId = userId,
                Status = "online",
                ConnectedAt = presence.ConnectedAt
            });

            _logger.LogInformation("User {UserId} is now online", userId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId != null)
        {
            _onlineUsers.TryRemove(userId, out _);

            // Notify all clients about the offline user
            await Clients.All.SendAsync("UserDisconnected", new
            {
                UserId = userId,
                DisconnectedAt = DateTime.UtcNow
            });

            _logger.LogInformation("User {UserId} is now offline", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Update user status (online, away, busy, etc.)
    /// </summary>
    public async Task UpdateStatus(string status)
    {
        var userId = GetUserId();
        if (userId != null && _onlineUsers.TryGetValue(userId, out var presence))
        {
            presence.Status = status;
            presence.LastActivity = DateTime.UtcNow;

            await Clients.All.SendAsync("UserStatusChanged", new
            {
                UserId = userId,
                Status = status,
                LastActivity = presence.LastActivity
            });
        }
    }

    /// <summary>
    /// Get list of online users
    /// </summary>
    public async Task GetOnlineUsers()
    {
        var onlineList = _onlineUsers.Values.Select(p => new
        {
            p.UserId,
            p.Status,
            p.ConnectedAt,
            p.LastActivity
        }).ToList();

        await Clients.Caller.SendAsync("OnlineUsersList", onlineList);
    }

    /// <summary>
    /// Check if specific users are online
    /// </summary>
    public async Task CheckUsersOnline(string[] userIds)
    {
        var results = userIds.Select(id => new
        {
            UserId = id,
            IsOnline = _onlineUsers.ContainsKey(id),
            Status = _onlineUsers.TryGetValue(id, out var presence) ? presence.Status : "offline"
        }).ToList();

        await Clients.Caller.SendAsync("UsersOnlineStatus", results);
    }

    /// <summary>
    /// Heartbeat to maintain connection
    /// </summary>
    public async Task Heartbeat()
    {
        var userId = GetUserId();
        if (userId != null && _onlineUsers.TryGetValue(userId, out var presence))
        {
            presence.LastActivity = DateTime.UtcNow;
        }
        await Task.CompletedTask;
    }

    private string? GetUserId()
    {
        return Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? Context.User?.FindFirst("sub")?.Value;
    }
}

public class UserPresence
{
    public string UserId { get; set; } = string.Empty;
    public string ConnectionId { get; set; } = string.Empty;
    public DateTime ConnectedAt { get; set; }
    public DateTime LastActivity { get; set; }
    public string Status { get; set; } = "online";
}
