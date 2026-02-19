using Marketplace.Database.Entities.Social;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marketplace.Database.Configurations.Social;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("conversations");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Title).HasColumnName("title").HasMaxLength(200);
        builder.Property(x => x.Type).HasColumnName("type").IsRequired();
        builder.Property(x => x.ProjectId).HasColumnName("project_id");
        builder.Property(x => x.OrderId).HasColumnName("order_id");
        builder.Property(x => x.LastMessageId).HasColumnName("last_message_id");
        builder.Property(x => x.LastMessageAt).HasColumnName("last_message_at");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();
        builder.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);

        builder.HasIndex(x => x.ProjectId);
        builder.HasIndex(x => x.OrderId);
        builder.HasIndex(x => x.LastMessageAt);
    }
}

public class ConversationParticipantConfiguration : IEntityTypeConfiguration<ConversationParticipant>
{
    public void Configure(EntityTypeBuilder<ConversationParticipant> builder)
    {
        builder.ToTable("conversation_participants");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.ConversationId).HasColumnName("conversation_id").IsRequired();
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.Role).HasColumnName("role").IsRequired();
        builder.Property(x => x.LastReadMessageId).HasColumnName("last_read_message_id");
        builder.Property(x => x.LastReadAt).HasColumnName("last_read_at");
        builder.Property(x => x.UnreadCount).HasColumnName("unread_count").HasDefaultValue(0);
        builder.Property(x => x.IsMuted).HasColumnName("is_muted").HasDefaultValue(false);
        builder.Property(x => x.IsArchived).HasColumnName("is_archived").HasDefaultValue(false);
        builder.Property(x => x.JoinedAt).HasColumnName("joined_at").IsRequired();
        builder.Property(x => x.LeftAt).HasColumnName("left_at");

        builder.HasIndex(x => new { x.ConversationId, x.UserId }).IsUnique();
        builder.HasIndex(x => x.UserId);
    }
}

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("messages");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.ConversationId).HasColumnName("conversation_id").IsRequired();
        builder.Property(x => x.SenderId).HasColumnName("sender_id").IsRequired();
        builder.Property(x => x.Content).HasColumnName("content").HasMaxLength(10000).IsRequired();
        builder.Property(x => x.Type).HasColumnName("type").IsRequired();
        builder.Property(x => x.AttachmentUrl).HasColumnName("attachment_url").HasMaxLength(500);
        builder.Property(x => x.AttachmentName).HasColumnName("attachment_name").HasMaxLength(200);
        builder.Property(x => x.AttachmentSize).HasColumnName("attachment_size");
        builder.Property(x => x.ReplyToId).HasColumnName("reply_to_id");
        builder.Property(x => x.IsEdited).HasColumnName("is_edited").HasDefaultValue(false);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.EditedAt).HasColumnName("edited_at");
        builder.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(x => x.ConversationId);
        builder.HasIndex(x => x.SenderId);
        builder.HasIndex(x => x.CreatedAt);
    }
}
