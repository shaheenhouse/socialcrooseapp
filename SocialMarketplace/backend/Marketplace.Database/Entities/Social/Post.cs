namespace Marketplace.Database.Entities.Social;

/// <summary>
/// Social media post (like LinkedIn posts)
/// </summary>
public class Post
{
    public Guid Id { get; set; }
    public Guid AuthorId { get; set; }
    public Guid? PageId { get; set; } // If posted on behalf of a page
    public string Content { get; set; } = string.Empty;
    public PostType Type { get; set; }
    public PostVisibility Visibility { get; set; } = PostVisibility.Public;
    public string? MediaUrls { get; set; } // JSON array of media URLs
    public string? LinkPreview { get; set; } // JSON object with link preview data
    public Guid? SharedPostId { get; set; } // If this is a reshare
    public Guid? ParentPostId { get; set; } // For replies/comments
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public int ShareCount { get; set; }
    public int ViewCount { get; set; }
    public bool IsEdited { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? EditedAt { get; set; }
    public bool IsActive { get; set; } = true;
}

public enum PostType
{
    Text = 0,
    Image = 1,
    Video = 2,
    Article = 3,
    Poll = 4,
    Event = 5,
    Job = 6,
    Document = 7,
    Celebration = 8
}

public enum PostVisibility
{
    Public = 0,
    ConnectionsOnly = 1,
    Private = 2
}
