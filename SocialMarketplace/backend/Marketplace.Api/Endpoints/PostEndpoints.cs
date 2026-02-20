using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Marketplace.Database;
using Marketplace.Database.Entities.Social;

namespace Marketplace.Api.Endpoints;

public static class PostEndpoints
{
    public static void MapPostEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/posts")
            .WithTags("Posts")
            .RequireAuthorization();

        group.MapGet("/feed", async (
            HttpContext context, MarketplaceDbContext db,
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        {
            var skip = (page - 1) * pageSize;
            var posts = await db.Posts.AsNoTracking()
                .Where(p => p.ParentPostId == null && p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .Skip(skip).Take(pageSize)
                .Join(db.Users.AsNoTracking(), p => p.AuthorId, u => u.Id,
                    (p, u) => new
                    {
                        p.Id, p.AuthorId,
                        Author = new { u.Id, u.FirstName, u.LastName, u.Username, u.AvatarUrl },
                        p.Content, p.Type, p.Visibility, p.MediaUrls, p.LinkPreview,
                        p.SharedPostId, p.LikeCount, p.CommentCount, p.ShareCount,
                        p.ViewCount, p.IsEdited, p.CreatedAt, p.EditedAt
                    })
                .ToListAsync();
            var total = await db.Posts.AsNoTracking().CountAsync(p => p.ParentPostId == null && p.IsActive);
            return Results.Ok(new { items = posts, total, page, pageSize });
        }).WithName("GetFeedPosts").WithSummary("Get feed posts paginated");

        group.MapGet("/{id:guid}", async (Guid id, HttpContext context, MarketplaceDbContext db) =>
        {
            var post = await db.Posts.AsNoTracking()
                .Where(p => p.Id == id && p.IsActive)
                .Join(db.Users.AsNoTracking(), p => p.AuthorId, u => u.Id,
                    (p, u) => new
                    {
                        p.Id, p.AuthorId,
                        Author = new { u.Id, u.FirstName, u.LastName, u.Username, u.AvatarUrl },
                        p.Content, p.Type, p.Visibility, p.MediaUrls, p.LinkPreview,
                        p.SharedPostId, p.ParentPostId, p.LikeCount, p.CommentCount,
                        p.ShareCount, p.ViewCount, p.IsEdited, p.CreatedAt, p.EditedAt
                    })
                .FirstOrDefaultAsync();
            return post == null ? Results.NotFound() : Results.Ok(post);
        }).WithName("GetPostById").WithSummary("Get a post by ID");

        group.MapGet("/user/{userId:guid}", async (
            Guid userId, HttpContext context, MarketplaceDbContext db,
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        {
            var skip = (page - 1) * pageSize;
            var posts = await db.Posts.AsNoTracking()
                .Where(p => p.AuthorId == userId && p.ParentPostId == null && p.IsActive)
                .OrderByDescending(p => p.CreatedAt).Skip(skip).Take(pageSize)
                .Join(db.Users.AsNoTracking(), p => p.AuthorId, u => u.Id,
                    (p, u) => new
                    {
                        p.Id, p.AuthorId,
                        Author = new { u.Id, u.FirstName, u.LastName, u.Username, u.AvatarUrl },
                        p.Content, p.Type, p.Visibility, p.MediaUrls,
                        p.LikeCount, p.CommentCount, p.ShareCount, p.ViewCount, p.CreatedAt, p.EditedAt
                    })
                .ToListAsync();
            var total = await db.Posts.AsNoTracking()
                .CountAsync(p => p.AuthorId == userId && p.ParentPostId == null && p.IsActive);
            return Results.Ok(new { items = posts, total, page, pageSize });
        }).WithName("GetPostsByUser").WithSummary("Get posts by user");

        group.MapPost("/", async ([FromBody] CreatePostRequest req, HttpContext context, MarketplaceDbContext db) =>
        {
            var userId = GetUserId(context);
            var post = new Post
            {
                Id = Guid.NewGuid(), AuthorId = userId, Content = req.Content,
                Type = req.Type, Visibility = req.Visibility,
                MediaUrls = req.MediaUrls, LinkPreview = req.LinkPreview,
                CreatedAt = DateTime.UtcNow, IsActive = true
            };
            db.Posts.Add(post);
            await db.SaveChangesAsync();
            return Results.Created($"/api/posts/{post.Id}", new { post.Id, post.AuthorId, post.Content, post.Type, post.CreatedAt });
        }).WithName("CreatePost").WithSummary("Create a new post");

        group.MapPatch("/{id:guid}", async (Guid id, [FromBody] UpdatePostRequest req, HttpContext context, MarketplaceDbContext db) =>
        {
            var userId = GetUserId(context);
            var post = await db.Posts.FirstOrDefaultAsync(p => p.Id == id && p.AuthorId == userId && p.IsActive);
            if (post == null) return Results.NotFound();
            if (req.Content != null) post.Content = req.Content;
            if (req.Visibility.HasValue) post.Visibility = req.Visibility.Value;
            if (req.MediaUrls != null) post.MediaUrls = req.MediaUrls;
            post.IsEdited = true;
            post.EditedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.Ok(new { post.Id, post.Content, post.Visibility, post.EditedAt });
        }).WithName("UpdatePost").WithSummary("Update a post");

        group.MapDelete("/{id:guid}", async (Guid id, HttpContext context, MarketplaceDbContext db) =>
        {
            var userId = GetUserId(context);
            var post = await db.Posts.FirstOrDefaultAsync(p => p.Id == id && p.AuthorId == userId && p.IsActive);
            if (post == null) return Results.NotFound();
            post.IsActive = false;
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).WithName("DeletePost").WithSummary("Soft delete a post");

        group.MapPost("/{id:guid}/reactions", async (Guid id, [FromBody] AddReactionRequest req, HttpContext context, MarketplaceDbContext db) =>
        {
            var userId = GetUserId(context);
            if (!await db.Posts.AsNoTracking().AnyAsync(p => p.Id == id && p.IsActive))
                return Results.NotFound();
            var existing = await db.PostReactions.FirstOrDefaultAsync(r => r.PostId == id && r.UserId == userId);
            if (existing != null) { existing.Type = req.Type; }
            else
            {
                db.PostReactions.Add(new PostReaction { Id = Guid.NewGuid(), PostId = id, UserId = userId, Type = req.Type, CreatedAt = DateTime.UtcNow });
                var post = await db.Posts.FirstAsync(p => p.Id == id);
                post.LikeCount++;
            }
            await db.SaveChangesAsync();
            return Results.Ok(new { postId = id, type = req.Type });
        }).WithName("AddPostReaction").WithSummary("Add or update a reaction");

        group.MapDelete("/{id:guid}/reactions", async (Guid id, HttpContext context, MarketplaceDbContext db) =>
        {
            var userId = GetUserId(context);
            var reaction = await db.PostReactions.FirstOrDefaultAsync(r => r.PostId == id && r.UserId == userId);
            if (reaction == null) return Results.NotFound();
            db.PostReactions.Remove(reaction);
            var post = await db.Posts.FirstOrDefaultAsync(p => p.Id == id);
            if (post != null && post.LikeCount > 0) post.LikeCount--;
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).WithName("RemovePostReaction").WithSummary("Remove a reaction");

        group.MapGet("/{id:guid}/comments", async (
            Guid id, HttpContext context, MarketplaceDbContext db,
            [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        {
            var skip = (page - 1) * pageSize;
            var comments = await db.Posts.AsNoTracking()
                .Where(p => p.ParentPostId == id && p.IsActive)
                .OrderByDescending(p => p.CreatedAt).Skip(skip).Take(pageSize)
                .Join(db.Users.AsNoTracking(), p => p.AuthorId, u => u.Id,
                    (p, u) => new
                    {
                        p.Id, p.AuthorId,
                        Author = new { u.Id, u.FirstName, u.LastName, u.Username, u.AvatarUrl },
                        p.Content, p.LikeCount, p.CreatedAt, p.IsEdited, p.EditedAt
                    })
                .ToListAsync();
            var total = await db.Posts.AsNoTracking().CountAsync(p => p.ParentPostId == id && p.IsActive);
            return Results.Ok(new { items = comments, total, page, pageSize });
        }).WithName("GetPostComments").WithSummary("Get comments on a post");

        group.MapPost("/{id:guid}/comments", async (Guid id, [FromBody] CreateCommentRequest req, HttpContext context, MarketplaceDbContext db) =>
        {
            var userId = GetUserId(context);
            if (!await db.Posts.AsNoTracking().AnyAsync(p => p.Id == id && p.IsActive))
                return Results.NotFound();
            var comment = new Post
            {
                Id = Guid.NewGuid(), AuthorId = userId, ParentPostId = id,
                Content = req.Content, Type = PostType.Text,
                CreatedAt = DateTime.UtcNow, IsActive = true
            };
            db.Posts.Add(comment);
            var parent = await db.Posts.FirstAsync(p => p.Id == id);
            parent.CommentCount++;
            await db.SaveChangesAsync();
            return Results.Created($"/api/posts/{comment.Id}", new { comment.Id, comment.AuthorId, comment.Content, comment.ParentPostId, comment.CreatedAt });
        }).WithName("AddPostComment").WithSummary("Add a comment to a post");
    }

    private static Guid GetUserId(HttpContext context)
    {
        var claim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return claim != null ? Guid.Parse(claim) : throw new UnauthorizedAccessException();
    }
}

public record CreatePostRequest(string Content, PostType Type = PostType.Text,
    PostVisibility Visibility = PostVisibility.Public, string? MediaUrls = null, string? LinkPreview = null);
public record UpdatePostRequest(string? Content = null, PostVisibility? Visibility = null, string? MediaUrls = null);
public record AddReactionRequest(ReactionType Type);
public record CreateCommentRequest(string Content);
