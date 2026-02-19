using Dapper;
using Marketplace.Core.Infrastructure;
using Marketplace.Database.Entities.Social;

namespace Marketplace.Slices.Social.Connections;

public interface IConnectionRepository
{
    Task<Connection?> GetByIdAsync(Guid id);
    Task<Connection?> GetConnectionBetweenUsersAsync(Guid userId1, Guid userId2);
    Task<IEnumerable<Connection>> GetConnectionsAsync(Guid userId, ConnectionStatus? status = null, int page = 1, int pageSize = 20);
    Task<IEnumerable<Connection>> GetPendingRequestsAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<IEnumerable<Connection>> GetSentRequestsAsync(Guid userId, int page = 1, int pageSize = 20);
    Task<int> GetConnectionCountAsync(Guid userId);
    Task<int> GetMutualConnectionsCountAsync(Guid userId1, Guid userId2);
    Task<IEnumerable<Guid>> GetMutualConnectionsAsync(Guid userId1, Guid userId2, int limit = 10);
    Task<IEnumerable<Guid>> GetConnectionSuggestionsAsync(Guid userId, int limit = 20);
    Task<Guid> CreateAsync(Connection connection);
    Task UpdateStatusAsync(Guid id, ConnectionStatus status);
    Task<bool> AreConnectedAsync(Guid userId1, Guid userId2);
}

public class ConnectionRepository : IConnectionRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public ConnectionRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Connection?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateReadConnection();
        return await connection.QueryFirstOrDefaultAsync<Connection>(
            "SELECT * FROM connections WHERE id = @Id",
            new { Id = id });
    }

    public async Task<Connection?> GetConnectionBetweenUsersAsync(Guid userId1, Guid userId2)
    {
        using var connection = _connectionFactory.CreateReadConnection();
        return await connection.QueryFirstOrDefaultAsync<Connection>(
            @"SELECT * FROM connections 
              WHERE (requester_id = @UserId1 AND addressee_id = @UserId2)
                 OR (requester_id = @UserId2 AND addressee_id = @UserId1)",
            new { UserId1 = userId1, UserId2 = userId2 });
    }

    public async Task<IEnumerable<Connection>> GetConnectionsAsync(Guid userId, ConnectionStatus? status = null, int page = 1, int pageSize = 20)
    {
        using var connection = _connectionFactory.CreateReadConnection();
        var offset = (page - 1) * pageSize;
        
        var sql = @"SELECT * FROM connections 
                    WHERE (requester_id = @UserId OR addressee_id = @UserId)
                    AND (@Status IS NULL OR status = @Status)
                    ORDER BY created_at DESC
                    LIMIT @PageSize OFFSET @Offset";
        
        return await connection.QueryAsync<Connection>(sql, 
            new { UserId = userId, Status = status, PageSize = pageSize, Offset = offset });
    }

    public async Task<IEnumerable<Connection>> GetPendingRequestsAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        using var connection = _connectionFactory.CreateReadConnection();
        var offset = (page - 1) * pageSize;
        
        return await connection.QueryAsync<Connection>(
            @"SELECT * FROM connections 
              WHERE addressee_id = @UserId AND status = @Status
              ORDER BY created_at DESC
              LIMIT @PageSize OFFSET @Offset",
            new { UserId = userId, Status = ConnectionStatus.Pending, PageSize = pageSize, Offset = offset });
    }

    public async Task<IEnumerable<Connection>> GetSentRequestsAsync(Guid userId, int page = 1, int pageSize = 20)
    {
        using var connection = _connectionFactory.CreateReadConnection();
        var offset = (page - 1) * pageSize;
        
        return await connection.QueryAsync<Connection>(
            @"SELECT * FROM connections 
              WHERE requester_id = @UserId AND status = @Status
              ORDER BY created_at DESC
              LIMIT @PageSize OFFSET @Offset",
            new { UserId = userId, Status = ConnectionStatus.Pending, PageSize = pageSize, Offset = offset });
    }

    public async Task<int> GetConnectionCountAsync(Guid userId)
    {
        using var connection = _connectionFactory.CreateReadConnection();
        return await connection.ExecuteScalarAsync<int>(
            @"SELECT COUNT(*) FROM connections 
              WHERE (requester_id = @UserId OR addressee_id = @UserId)
              AND status = @Status",
            new { UserId = userId, Status = ConnectionStatus.Accepted });
    }

    public async Task<int> GetMutualConnectionsCountAsync(Guid userId1, Guid userId2)
    {
        using var connection = _connectionFactory.CreateReadConnection();
        return await connection.ExecuteScalarAsync<int>(
            @"SELECT COUNT(*) FROM (
                SELECT CASE WHEN requester_id = @UserId1 THEN addressee_id ELSE requester_id END as connected_id
                FROM connections WHERE (requester_id = @UserId1 OR addressee_id = @UserId1) AND status = 1
              ) c1
              INNER JOIN (
                SELECT CASE WHEN requester_id = @UserId2 THEN addressee_id ELSE requester_id END as connected_id
                FROM connections WHERE (requester_id = @UserId2 OR addressee_id = @UserId2) AND status = 1
              ) c2 ON c1.connected_id = c2.connected_id",
            new { UserId1 = userId1, UserId2 = userId2 });
    }

    public async Task<IEnumerable<Guid>> GetMutualConnectionsAsync(Guid userId1, Guid userId2, int limit = 10)
    {
        using var connection = _connectionFactory.CreateReadConnection();
        return await connection.QueryAsync<Guid>(
            @"SELECT c1.connected_id FROM (
                SELECT CASE WHEN requester_id = @UserId1 THEN addressee_id ELSE requester_id END as connected_id
                FROM connections WHERE (requester_id = @UserId1 OR addressee_id = @UserId1) AND status = 1
              ) c1
              INNER JOIN (
                SELECT CASE WHEN requester_id = @UserId2 THEN addressee_id ELSE requester_id END as connected_id
                FROM connections WHERE (requester_id = @UserId2 OR addressee_id = @UserId2) AND status = 1
              ) c2 ON c1.connected_id = c2.connected_id
              LIMIT @Limit",
            new { UserId1 = userId1, UserId2 = userId2, Limit = limit });
    }

    public async Task<IEnumerable<Guid>> GetConnectionSuggestionsAsync(Guid userId, int limit = 20)
    {
        using var connection = _connectionFactory.CreateReadConnection();
        // Suggest connections of connections (2nd degree) not already connected
        return await connection.QueryAsync<Guid>(
            @"WITH user_connections AS (
                SELECT CASE WHEN requester_id = @UserId THEN addressee_id ELSE requester_id END as connected_id
                FROM connections WHERE (requester_id = @UserId OR addressee_id = @UserId) AND status = 1
              ),
              second_degree AS (
                SELECT DISTINCT CASE WHEN c.requester_id = uc.connected_id THEN c.addressee_id ELSE c.requester_id END as suggestion_id
                FROM connections c
                INNER JOIN user_connections uc ON (c.requester_id = uc.connected_id OR c.addressee_id = uc.connected_id)
                WHERE c.status = 1
              )
              SELECT suggestion_id FROM second_degree
              WHERE suggestion_id != @UserId
              AND suggestion_id NOT IN (SELECT connected_id FROM user_connections)
              AND NOT EXISTS (
                SELECT 1 FROM connections WHERE 
                  ((requester_id = @UserId AND addressee_id = suggestion_id) OR
                   (requester_id = suggestion_id AND addressee_id = @UserId))
              )
              LIMIT @Limit",
            new { UserId = userId, Limit = limit });
    }

    public async Task<Guid> CreateAsync(Connection conn)
    {
        using var connection = _connectionFactory.CreateWriteConnection();
        var id = Guid.NewGuid();
        await connection.ExecuteAsync(
            @"INSERT INTO connections (id, requester_id, addressee_id, status, message, created_at)
              VALUES (@Id, @RequesterId, @AddresseeId, @Status, @Message, @CreatedAt)",
            new { Id = id, conn.RequesterId, conn.AddresseeId, conn.Status, conn.Message, CreatedAt = DateTime.UtcNow });
        return id;
    }

    public async Task UpdateStatusAsync(Guid id, ConnectionStatus status)
    {
        using var connection = _connectionFactory.CreateWriteConnection();
        var updateField = status switch
        {
            ConnectionStatus.Accepted => "accepted_at",
            ConnectionStatus.Rejected => "rejected_at",
            ConnectionStatus.Blocked => "blocked_at",
            _ => null
        };

        var sql = updateField != null
            ? $"UPDATE connections SET status = @Status, {updateField} = @Now WHERE id = @Id"
            : "UPDATE connections SET status = @Status WHERE id = @Id";

        await connection.ExecuteAsync(sql, new { Id = id, Status = status, Now = DateTime.UtcNow });
    }

    public async Task<bool> AreConnectedAsync(Guid userId1, Guid userId2)
    {
        using var connection = _connectionFactory.CreateReadConnection();
        var count = await connection.ExecuteScalarAsync<int>(
            @"SELECT COUNT(*) FROM connections 
              WHERE ((requester_id = @UserId1 AND addressee_id = @UserId2)
                 OR (requester_id = @UserId2 AND addressee_id = @UserId1))
              AND status = @Status",
            new { UserId1 = userId1, UserId2 = userId2, Status = ConnectionStatus.Accepted });
        return count > 0;
    }
}
