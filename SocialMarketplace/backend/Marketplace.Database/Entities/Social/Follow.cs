namespace Marketplace.Database.Entities.Social;

/// <summary>
/// Follow relationship (user follows user, company, store, page)
/// </summary>
public class Follow
{
    public Guid Id { get; set; }
    public Guid FollowerId { get; set; }
    public Guid FollowingId { get; set; }
    public FollowTargetType TargetType { get; set; }
    public bool NotificationsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

public enum FollowTargetType
{
    User = 0,
    Store = 1,
    Company = 2,
    Page = 3,
    Project = 4,
    Hashtag = 5
}
