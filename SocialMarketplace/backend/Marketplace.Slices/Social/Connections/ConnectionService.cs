using Marketplace.Core.Infrastructure;
using Marketplace.Database.Entities.Social;

namespace Marketplace.Slices.Social.Connections;

public interface IConnectionService
{
    Task<ConnectionDto?> GetConnectionAsync(Guid connectionId, Guid currentUserId);
    Task<ConnectionStatusDto> GetConnectionStatusAsync(Guid userId1, Guid userId2);
    Task<PaginatedResult<ConnectionDto>> GetMyConnectionsAsync(Guid userId, ConnectionStatus? status, int page, int pageSize);
    Task<PaginatedResult<ConnectionDto>> GetPendingRequestsAsync(Guid userId, int page, int pageSize);
    Task<PaginatedResult<ConnectionDto>> GetSentRequestsAsync(Guid userId, int page, int pageSize);
    Task<ConnectionStatsDto> GetConnectionStatsAsync(Guid userId);
    Task<IEnumerable<ConnectionSuggestionDto>> GetSuggestionsAsync(Guid userId, int limit);
    Task<Guid> SendConnectionRequestAsync(Guid requesterId, Guid addresseeId, string? message);
    Task AcceptConnectionAsync(Guid connectionId, Guid userId);
    Task RejectConnectionAsync(Guid connectionId, Guid userId);
    Task WithdrawConnectionAsync(Guid connectionId, Guid userId);
    Task RemoveConnectionAsync(Guid connectionId, Guid userId);
    Task BlockUserAsync(Guid userId, Guid blockedUserId);
}

public class ConnectionService : IConnectionService
{
    private readonly IConnectionRepository _repository;
    private readonly IJobQueue _jobQueue;

    public ConnectionService(IConnectionRepository repository, IJobQueue jobQueue)
    {
        _repository = repository;
        _jobQueue = jobQueue;
    }

    public async Task<ConnectionDto?> GetConnectionAsync(Guid connectionId, Guid currentUserId)
    {
        var conn = await _repository.GetByIdAsync(connectionId);
        if (conn == null) return null;
        
        // Verify user is part of this connection
        if (conn.RequesterId != currentUserId && conn.AddresseeId != currentUserId)
            return null;

        return MapToDto(conn, currentUserId);
    }

    public async Task<ConnectionStatusDto> GetConnectionStatusAsync(Guid userId1, Guid userId2)
    {
        var conn = await _repository.GetConnectionBetweenUsersAsync(userId1, userId2);
        var mutualCount = await _repository.GetMutualConnectionsCountAsync(userId1, userId2);

        return new ConnectionStatusDto
        {
            IsConnected = conn?.Status == ConnectionStatus.Accepted,
            IsPending = conn?.Status == ConnectionStatus.Pending,
            IsBlocked = conn?.Status == ConnectionStatus.Blocked,
            ConnectionId = conn?.Id,
            MutualConnectionsCount = mutualCount,
            IsOutgoingRequest = conn?.RequesterId == userId1 && conn.Status == ConnectionStatus.Pending
        };
    }

    public async Task<PaginatedResult<ConnectionDto>> GetMyConnectionsAsync(Guid userId, ConnectionStatus? status, int page, int pageSize)
    {
        var connections = await _repository.GetConnectionsAsync(userId, status ?? ConnectionStatus.Accepted, page, pageSize);
        var total = await _repository.GetConnectionCountAsync(userId);

        return new PaginatedResult<ConnectionDto>
        {
            Items = connections.Select(c => MapToDto(c, userId)),
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PaginatedResult<ConnectionDto>> GetPendingRequestsAsync(Guid userId, int page, int pageSize)
    {
        var connections = await _repository.GetPendingRequestsAsync(userId, page, pageSize);
        return new PaginatedResult<ConnectionDto>
        {
            Items = connections.Select(c => MapToDto(c, userId)),
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PaginatedResult<ConnectionDto>> GetSentRequestsAsync(Guid userId, int page, int pageSize)
    {
        var connections = await _repository.GetSentRequestsAsync(userId, page, pageSize);
        return new PaginatedResult<ConnectionDto>
        {
            Items = connections.Select(c => MapToDto(c, userId)),
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ConnectionStatsDto> GetConnectionStatsAsync(Guid userId)
    {
        var count = await _repository.GetConnectionCountAsync(userId);
        var pending = (await _repository.GetPendingRequestsAsync(userId, 1, 1000)).Count();
        var sent = (await _repository.GetSentRequestsAsync(userId, 1, 1000)).Count();

        return new ConnectionStatsDto
        {
            TotalConnections = count,
            PendingRequests = pending,
            SentRequests = sent
        };
    }

    public async Task<IEnumerable<ConnectionSuggestionDto>> GetSuggestionsAsync(Guid userId, int limit)
    {
        var suggestions = await _repository.GetConnectionSuggestionsAsync(userId, limit);
        return suggestions.Select(s => new ConnectionSuggestionDto
        {
            UserId = s,
            MutualConnections = 0 // Would need another query for each
        });
    }

    public async Task<Guid> SendConnectionRequestAsync(Guid requesterId, Guid addresseeId, string? message)
    {
        // Check if connection already exists
        var existing = await _repository.GetConnectionBetweenUsersAsync(requesterId, addresseeId);
        if (existing != null)
        {
            if (existing.Status == ConnectionStatus.Blocked)
                throw new InvalidOperationException("Cannot send connection request to this user");
            if (existing.Status == ConnectionStatus.Accepted)
                throw new InvalidOperationException("Already connected with this user");
            if (existing.Status == ConnectionStatus.Pending)
                throw new InvalidOperationException("Connection request already pending");
        }

        var connection = new Connection
        {
            RequesterId = requesterId,
            AddresseeId = addresseeId,
            Status = ConnectionStatus.Pending,
            Message = message
        };

        var id = await _repository.CreateAsync(connection);

        // Queue notification
        await _jobQueue.EnqueueAsync("notifications", new Dictionary<string, string>
        {
            ["Type"] = "new_connection_request",
            ["UserId"] = addresseeId.ToString(),
            ["RequesterId"] = requesterId.ToString(),
            ["ConnectionId"] = id.ToString()
        });

        return id;
    }

    public async Task AcceptConnectionAsync(Guid connectionId, Guid userId)
    {
        var conn = await _repository.GetByIdAsync(connectionId);
        if (conn == null || conn.AddresseeId != userId)
            throw new InvalidOperationException("Invalid connection request");
        
        if (conn.Status != ConnectionStatus.Pending)
            throw new InvalidOperationException("Connection request is not pending");

        await _repository.UpdateStatusAsync(connectionId, ConnectionStatus.Accepted);

        // Queue notification
        await _jobQueue.EnqueueAsync("notifications", new Dictionary<string, string>
        {
            ["Type"] = "connection_accepted",
            ["UserId"] = conn.RequesterId.ToString(),
            ["AccepterId"] = userId.ToString(),
            ["ConnectionId"] = connectionId.ToString()
        });
    }

    public async Task RejectConnectionAsync(Guid connectionId, Guid userId)
    {
        var conn = await _repository.GetByIdAsync(connectionId);
        if (conn == null || conn.AddresseeId != userId)
            throw new InvalidOperationException("Invalid connection request");

        await _repository.UpdateStatusAsync(connectionId, ConnectionStatus.Rejected);
    }

    public async Task WithdrawConnectionAsync(Guid connectionId, Guid userId)
    {
        var conn = await _repository.GetByIdAsync(connectionId);
        if (conn == null || conn.RequesterId != userId)
            throw new InvalidOperationException("Invalid connection request");

        await _repository.UpdateStatusAsync(connectionId, ConnectionStatus.Withdrawn);
    }

    public async Task RemoveConnectionAsync(Guid connectionId, Guid userId)
    {
        var conn = await _repository.GetByIdAsync(connectionId);
        if (conn == null || (conn.RequesterId != userId && conn.AddresseeId != userId))
            throw new InvalidOperationException("Invalid connection");

        await _repository.UpdateStatusAsync(connectionId, ConnectionStatus.Withdrawn);
    }

    public async Task BlockUserAsync(Guid userId, Guid blockedUserId)
    {
        var existing = await _repository.GetConnectionBetweenUsersAsync(userId, blockedUserId);
        if (existing != null)
        {
            await _repository.UpdateStatusAsync(existing.Id, ConnectionStatus.Blocked);
        }
        else
        {
            var connection = new Connection
            {
                RequesterId = userId,
                AddresseeId = blockedUserId,
                Status = ConnectionStatus.Blocked
            };
            await _repository.CreateAsync(connection);
        }
    }

    private static ConnectionDto MapToDto(Connection conn, Guid currentUserId)
    {
        var otherUserId = conn.RequesterId == currentUserId ? conn.AddresseeId : conn.RequesterId;
        return new ConnectionDto
        {
            Id = conn.Id,
            OtherUserId = otherUserId,
            Status = conn.Status,
            Message = conn.Message,
            IsOutgoing = conn.RequesterId == currentUserId,
            CreatedAt = conn.CreatedAt,
            AcceptedAt = conn.AcceptedAt
        };
    }
}

public record ConnectionDto
{
    public Guid Id { get; init; }
    public Guid OtherUserId { get; init; }
    public ConnectionStatus Status { get; init; }
    public string? Message { get; init; }
    public bool IsOutgoing { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? AcceptedAt { get; init; }
}

public record ConnectionStatusDto
{
    public bool IsConnected { get; init; }
    public bool IsPending { get; init; }
    public bool IsBlocked { get; init; }
    public Guid? ConnectionId { get; init; }
    public int MutualConnectionsCount { get; init; }
    public bool IsOutgoingRequest { get; init; }
}

public record ConnectionStatsDto
{
    public int TotalConnections { get; init; }
    public int PendingRequests { get; init; }
    public int SentRequests { get; init; }
}

public record ConnectionSuggestionDto
{
    public Guid UserId { get; init; }
    public int MutualConnections { get; init; }
}

public record PaginatedResult<T>
{
    public IEnumerable<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
