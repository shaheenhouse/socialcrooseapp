using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Marketplace.Database.Entities;

namespace Marketplace.Database.Configurations;

public class ChatRoomConfiguration : IEntityTypeConfiguration<ChatRoom>
{
    public void Configure(EntityTypeBuilder<ChatRoom> builder)
    {
        builder.ToTable("chat_rooms");

        builder.HasKey(cr => cr.Id);

        builder.Property(cr => cr.Name).HasMaxLength(255);
        builder.Property(cr => cr.Type).HasMaxLength(20).HasDefaultValue("Direct");
        builder.Property(cr => cr.ImageUrl).HasMaxLength(500);
        builder.Property(cr => cr.LastMessagePreview).HasMaxLength(500);

        builder.HasIndex(cr => cr.ProjectId);
        builder.HasIndex(cr => cr.OrderId);
        builder.HasIndex(cr => cr.StoreId);
        builder.HasIndex(cr => cr.LastMessageAt);
    }
}

public class ChatParticipantConfiguration : IEntityTypeConfiguration<ChatParticipant>
{
    public void Configure(EntityTypeBuilder<ChatParticipant> builder)
    {
        builder.ToTable("chat_participants");

        builder.HasKey(cp => cp.Id);

        builder.Property(cp => cp.Role).HasMaxLength(20).HasDefaultValue("Member");
        builder.Property(cp => cp.Nickname).HasMaxLength(100);

        builder.HasIndex(cp => new { cp.ChatRoomId, cp.UserId }).IsUnique();
        builder.HasIndex(cp => cp.UserId);

        builder.HasOne(cp => cp.ChatRoom)
            .WithMany(cr => cr.Participants)
            .HasForeignKey(cp => cp.ChatRoomId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cp => cp.User)
            .WithMany()
            .HasForeignKey(cp => cp.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("chat_messages");

        builder.HasKey(cm => cm.Id);

        builder.Property(cm => cm.Content).IsRequired();
        builder.Property(cm => cm.Type).HasMaxLength(20).HasDefaultValue("Text");

        builder.HasIndex(cm => cm.ChatRoomId);
        builder.HasIndex(cm => cm.SenderId);
        builder.HasIndex(cm => cm.CreatedAt);
        builder.HasIndex(cm => cm.IsPinned);
        builder.HasIndex(cm => new { cm.ChatRoomId, cm.CreatedAt });

        builder.HasOne(cm => cm.ChatRoom)
            .WithMany(cr => cr.Messages)
            .HasForeignKey(cm => cm.ChatRoomId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cm => cm.Sender)
            .WithMany()
            .HasForeignKey(cm => cm.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cm => cm.ReplyTo)
            .WithMany()
            .HasForeignKey(cm => cm.ReplyToId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
