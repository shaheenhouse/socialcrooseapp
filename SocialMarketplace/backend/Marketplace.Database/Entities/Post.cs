namespace Marketplace.Database.Entities;

public class Post : BaseEntity
{
    public Guid AuthorId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Type { get; set; } = "text"; // text | image | video | article | poll | event | job
    public string Visibility { get; set; } = "public"; // public | connections | private
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; }
    public string? LinkUrl { get; set; }
    public string? LinkTitle { get; set; }
    public string? LinkDescription { get; set; }
    public Guid? SharedPostId { get; set; }
    public int ReactionsCount { get; set; }
    public int CommentsCount { get; set; }
    public int SharesCount { get; set; }
    public int ViewsCount { get; set; }
    public bool IsPinned { get; set; }

    public User? Author { get; set; }
    public Post? SharedPost { get; set; }
    public ICollection<PostReaction> Reactions { get; set; } = new List<PostReaction>();
    public ICollection<PostComment> Comments { get; set; } = new List<PostComment>();
}

public class PostReaction : BaseEntity
{
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public string Type { get; set; } = "like"; // like | love | celebrate | support | insightful | funny

    public Post? Post { get; set; }
    public User? User { get; set; }
}

public class PostComment : BaseEntity
{
    public Guid PostId { get; set; }
    public Guid UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public int ReactionsCount { get; set; }

    public Post? Post { get; set; }
    public User? User { get; set; }
    public PostComment? Parent { get; set; }
    public ICollection<PostComment> Replies { get; set; } = new List<PostComment>();
}
