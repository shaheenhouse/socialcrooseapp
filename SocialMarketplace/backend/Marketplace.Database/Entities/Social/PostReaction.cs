namespace Marketplace.Database.Entities.Social;

/// <summary>
/// Reaction to a post (like, celebrate, love, etc.)
/// </summary>
public class PostReaction
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public ReactionType Type { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum ReactionType
{
    Like = 0,
    Love = 1,
    Celebrate = 2,
    Support = 3,
    Insightful = 4,
    Funny = 5
}
