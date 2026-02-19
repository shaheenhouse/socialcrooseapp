using Dapper;
using Marketplace.Core.Infrastructure;
using Marketplace.Database.Entities.Social;

namespace Marketplace.Slices.Social.Follows;

public interface IFollowRepository
{
    Task<Follow?> GetByIdAsync(Guid id);
    Task<Follow?> GetFollowAsync(Guid followerId, Guid followingId, FollowTargetType targetType);
    Task<IEnumerable<Follow>> GetFollowersAsync(Guid targetId, FollowTargetType targetType, int page, int pageSize);
    Task<IEnumerable<Follow>> GetFollowingAsync(Guid followerId, FollowTargetType? targetType, int page, int pageSize);
    Task<int> GetFollowerCountAsync(Guid targetId, FollowTargetType targetType);
    Task<int> GetFollowingCountAsync(Guid followerId, FollowTargetType? targetType);
    Task<IEnumerable<Guid>> GetMutualFollowersAsync(Guid userId1, Guid userId2, int limit);
    Task<bool> IsFollowingAsync(Guid followerId, Guid followingId, FollowTargetType targetType);
    Task<Guid> CreateAsync(Follow follow);
    Task DeleteAsync(Guid id);
    Task UpdateNotificationsAsync(Guid id, bool enabled);
}

public class FollowRepository : IFollowRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public FollowRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Follow?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateReadConnection();
        return await connection.QueryFirstOrDefaultAsync<Follow>(
            "SELECT * FROM follows WHERE id = @Id",
            new { Id = id });
    }

    public async Task<Follow?> GetFollowAsync(Guid followerId, Guid followingId, FollowTargetType targetType)
    {
        using var connection = _connectionFactory.CreateReadConnection();
        return await connection.QueryFirstOrDefaultAsync<Follow>(
            @"SELECT * FROM follows 
              WHERE follower_id = @FollowerId 
              AND following_id = @FollowingId 
              AND target_type = @TargetType",
            new { FollowerId = followerId, FollowingId = followingId, TargetType = targetType });
    }

    public async Task<IEnumerable<Follow>> GetFollowersAsync(Guid targetId, FollowTargetType targetType, int page, int pageSize)
    {
        using var connection = _connectionFactory.CreateReadConnection();
        var offset = (page - 1) * pageSize;
        
        return await connection.QueryAsync<Follow>(
            @"SELECT * FROM follows 
              WHERE following_id = @TargetId AND target_type = @TargetType
              ORDER BY created_at DESC
              LIMIT @PageSize OFFSET @Offset",
            new { TargetId = targetId, TargetType = targetType, PageSize = pageSize, Offset = offset });
    }

    public async Task<IEnumerable<Follow>> GetFollowingAsync(Guid followerId, FollowTargetType? targetType, int page, int pageSize)
    {
        using var connection = _connectionFactory.CreateReadConnection();
        var offset = (page - 1) * pageSize;
        
        return await connection.QueryAsync<Follow>(
            @"SELECT * FROM follows 
              WHERE follower_id = @FollowerId 
              AND (@TargetType IS NULL OR target_type = @TargetType)
              ORDER BY created_at DESC
              LIMIT @PageSize OFFSET @Offset",
            new { FollowerId = followerId, TargetType = targetType, PageSize = pageSize, Offset = offset });
    }

    public async Task<int> GetFollowerCountAsync(Guid targetId, FollowTargetType targetType)
    {
        using var connection = _connectionFactory.CreateReadConnection();
        return await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM follows WHERE following_id = @TargetId AND target_type = @TargetType",
            new { TargetId = targetId, TargetType = targetType });
    }

    public async Task<int> GetFollowingCountAsync(Guid followerId, FollowTargetType? targetType)
    {
        using var connection = _connectionFactory.CreateReadConnection();
        return await connection.ExecuteScalarAsync<int>(
            @"SELECT COUNT(*) FROM follows 
              WHERE follower_id = @FollowerId 
              AND (@TargetType IS NULL OR target_type = @TargetType)",
            new { FollowerId = followerId, TargetType = targetType });
    }

    public async Task<IEnumerable<Guid>> GetMutualFollowersAsync(Guid userId1, Guid userId2, int limit)
    {
        using var connection = _connectionFactory.CreateReadConnection();
        return await connection.QueryAsync<Guid>(
            @"SELECT f1.follower_id FROM follows f1
              INNER JOIN follows f2 ON f1.follower_id = f2.follower_id
              WHERE f1.following_id = @UserId1 AND f1.target_type = 0
              AND f2.following_id = @UserId2 AND f2.target_type = 0
              LIMIT @Limit",
            new { UserId1 = userId1, UserId2 = userId2, Limit = limit });
    }

    public async Task<bool> IsFollowingAsync(Guid followerId, Guid followingId, FollowTargetType targetType)
    {
        using var connection = _connectionFactory.CreateReadConnection();
        var count = await connection.ExecuteScalarAsync<int>(
            @"SELECT COUNT(*) FROM follows 
              WHERE follower_id = @FollowerId 
              AND following_id = @FollowingId 
              AND target_type = @TargetType",
            new { FollowerId = followerId, FollowingId = followingId, TargetType = targetType });
        return count > 0;
    }

    public async Task<Guid> CreateAsync(Follow follow)
    {
        using var connection = _connectionFactory.CreateWriteConnection();
        var id = Guid.NewGuid();
        await connection.ExecuteAsync(
            @"INSERT INTO follows (id, follower_id, following_id, target_type, notifications_enabled, created_at)
              VALUES (@Id, @FollowerId, @FollowingId, @TargetType, @NotificationsEnabled, @CreatedAt)",
            new { Id = id, follow.FollowerId, follow.FollowingId, follow.TargetType, follow.NotificationsEnabled, CreatedAt = DateTime.UtcNow });
        return id;
    }

    public async Task DeleteAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateWriteConnection();
        await connection.ExecuteAsync("DELETE FROM follows WHERE id = @Id", new { Id = id });
    }

    public async Task UpdateNotificationsAsync(Guid id, bool enabled)
    {
        using var connection = _connectionFactory.CreateWriteConnection();
        await connection.ExecuteAsync(
            "UPDATE follows SET notifications_enabled = @Enabled WHERE id = @Id",
            new { Id = id, Enabled = enabled });
    }
}
