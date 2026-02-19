using Marketplace.Database.Entities.Social;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marketplace.Database.Configurations.Social;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("posts");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.AuthorId).HasColumnName("author_id").IsRequired();
        builder.Property(x => x.PageId).HasColumnName("page_id");
        builder.Property(x => x.Content).HasColumnName("content").HasMaxLength(10000).IsRequired();
        builder.Property(x => x.Type).HasColumnName("type").IsRequired();
        builder.Property(x => x.Visibility).HasColumnName("visibility").HasDefaultValue(PostVisibility.Public);
        builder.Property(x => x.MediaUrls).HasColumnName("media_urls").HasColumnType("jsonb");
        builder.Property(x => x.LinkPreview).HasColumnName("link_preview").HasColumnType("jsonb");
        builder.Property(x => x.SharedPostId).HasColumnName("shared_post_id");
        builder.Property(x => x.ParentPostId).HasColumnName("parent_post_id");
        builder.Property(x => x.LikeCount).HasColumnName("like_count").HasDefaultValue(0);
        builder.Property(x => x.CommentCount).HasColumnName("comment_count").HasDefaultValue(0);
        builder.Property(x => x.ShareCount).HasColumnName("share_count").HasDefaultValue(0);
        builder.Property(x => x.ViewCount).HasColumnName("view_count").HasDefaultValue(0);
        builder.Property(x => x.IsEdited).HasColumnName("is_edited").HasDefaultValue(false);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.EditedAt).HasColumnName("edited_at");
        builder.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);

        builder.HasIndex(x => x.AuthorId);
        builder.HasIndex(x => x.PageId);
        builder.HasIndex(x => x.ParentPostId);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.IsActive);
    }
}

public class PostReactionConfiguration : IEntityTypeConfiguration<PostReaction>
{
    public void Configure(EntityTypeBuilder<PostReaction> builder)
    {
        builder.ToTable("post_reactions");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.PostId).HasColumnName("post_id").IsRequired();
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.Type).HasColumnName("type").IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(x => new { x.PostId, x.UserId }).IsUnique();
        builder.HasIndex(x => x.PostId);
        builder.HasIndex(x => x.UserId);
    }
}
