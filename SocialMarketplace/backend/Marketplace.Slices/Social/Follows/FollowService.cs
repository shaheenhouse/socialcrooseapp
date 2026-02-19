using Marketplace.Core.Infrastructure;
using Marketplace.Database.Entities.Social;

namespace Marketplace.Slices.Social.Follows;

public interface IFollowService
{
    Task<FollowStatusDto> GetFollowStatusAsync(Guid followerId, Guid followingId, FollowTargetType targetType);
    Task<FollowStatsDto> GetFollowStatsAsync(Guid userId);
    Task<IEnumerable<FollowerDto>> GetFollowersAsync(Guid targetId, FollowTargetType targetType, int page, int pageSize);
    Task<IEnumerable<FollowingDto>> GetFollowingAsync(Guid followerId, FollowTargetType? targetType, int page, int pageSize);
    Task<Guid> FollowAsync(Guid followerId, Guid followingId, FollowTargetType targetType);
    Task UnfollowAsync(Guid followerId, Guid followingId, FollowTargetType targetType);
    Task ToggleNotificationsAsync(Guid followerId, Guid followingId, FollowTargetType targetType, bool enabled);
}

public class FollowService : IFollowService
{
    private readonly IFollowRepository _repository;
    private readonly IJobQueue _jobQueue;

    public FollowService(IFollowRepository repository, IJobQueue jobQueue)
    {
        _repository = repository;
        _jobQueue = jobQueue;
    }

    public async Task<FollowStatusDto> GetFollowStatusAsync(Guid followerId, Guid followingId, FollowTargetType targetType)
    {
        var follow = await _repository.GetFollowAsync(followerId, followingId, targetType);
        var reverseFollow = targetType == FollowTargetType.User 
            ? await _repository.GetFollowAsync(followingId, followerId, targetType)
            : null;

        return new FollowStatusDto
        {
            IsFollowing = follow != null,
            IsFollowedBy = reverseFollow != null,
            NotificationsEnabled = follow?.NotificationsEnabled ?? false,
            FollowId = follow?.Id
        };
    }

    public async Task<FollowStatsDto> GetFollowStatsAsync(Guid userId)
    {
        var followers = await _repository.GetFollowerCountAsync(userId, FollowTargetType.User);
        var following = await _repository.GetFollowingCountAsync(userId, FollowTargetType.User);
        var followingPages = await _repository.GetFollowingCountAsync(userId, FollowTargetType.Page);
        var followingStores = await _repository.GetFollowingCountAsync(userId, FollowTargetType.Store);
        var followingCompanies = await _repository.GetFollowingCountAsync(userId, FollowTargetType.Company);

        return new FollowStatsDto
        {
            FollowersCount = followers,
            FollowingCount = following,
            FollowingPagesCount = followingPages,
            FollowingStoresCount = followingStores,
            FollowingCompaniesCount = followingCompanies
        };
    }

    public async Task<IEnumerable<FollowerDto>> GetFollowersAsync(Guid targetId, FollowTargetType targetType, int page, int pageSize)
    {
        var follows = await _repository.GetFollowersAsync(targetId, targetType, page, pageSize);
        return follows.Select(f => new FollowerDto
        {
            FollowId = f.Id,
            FollowerId = f.FollowerId,
            FollowedAt = f.CreatedAt
        });
    }

    public async Task<IEnumerable<FollowingDto>> GetFollowingAsync(Guid followerId, FollowTargetType? targetType, int page, int pageSize)
    {
        var follows = await _repository.GetFollowingAsync(followerId, targetType, page, pageSize);
        return follows.Select(f => new FollowingDto
        {
            FollowId = f.Id,
            FollowingId = f.FollowingId,
            TargetType = f.TargetType,
            NotificationsEnabled = f.NotificationsEnabled,
            FollowedAt = f.CreatedAt
        });
    }

    public async Task<Guid> FollowAsync(Guid followerId, Guid followingId, FollowTargetType targetType)
    {
        // Check if already following
        var existing = await _repository.GetFollowAsync(followerId, followingId, targetType);
        if (existing != null)
            throw new InvalidOperationException("Already following");

        var follow = new Follow
        {
            FollowerId = followerId,
            FollowingId = followingId,
            TargetType = targetType,
            NotificationsEnabled = true
        };

        var id = await _repository.CreateAsync(follow);

        // Queue notification for user follows
        if (targetType == FollowTargetType.User)
        {
            await _jobQueue.EnqueueAsync("notifications", new Dictionary<string, string>
            {
                ["Type"] = "new_follower",
                ["UserId"] = followingId.ToString(),
                ["FollowerId"] = followerId.ToString()
            });
        }

        return id;
    }

    public async Task UnfollowAsync(Guid followerId, Guid followingId, FollowTargetType targetType)
    {
        var follow = await _repository.GetFollowAsync(followerId, followingId, targetType);
        if (follow == null)
            throw new InvalidOperationException("Not following");

        await _repository.DeleteAsync(follow.Id);
    }

    public async Task ToggleNotificationsAsync(Guid followerId, Guid followingId, FollowTargetType targetType, bool enabled)
    {
        var follow = await _repository.GetFollowAsync(followerId, followingId, targetType);
        if (follow == null)
            throw new InvalidOperationException("Not following");

        await _repository.UpdateNotificationsAsync(follow.Id, enabled);
    }
}

public record FollowStatusDto
{
    public bool IsFollowing { get; init; }
    public bool IsFollowedBy { get; init; }
    public bool NotificationsEnabled { get; init; }
    public Guid? FollowId { get; init; }
}

public record FollowStatsDto
{
    public int FollowersCount { get; init; }
    public int FollowingCount { get; init; }
    public int FollowingPagesCount { get; init; }
    public int FollowingStoresCount { get; init; }
    public int FollowingCompaniesCount { get; init; }
}

public record FollowerDto
{
    public Guid FollowId { get; init; }
    public Guid FollowerId { get; init; }
    public DateTime FollowedAt { get; init; }
}

public record FollowingDto
{
    public Guid FollowId { get; init; }
    public Guid FollowingId { get; init; }
    public FollowTargetType TargetType { get; init; }
    public bool NotificationsEnabled { get; init; }
    public DateTime FollowedAt { get; init; }
}
