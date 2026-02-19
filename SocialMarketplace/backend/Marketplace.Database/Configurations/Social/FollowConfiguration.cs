using Marketplace.Database.Entities.Social;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marketplace.Database.Configurations.Social;

public class FollowConfiguration : IEntityTypeConfiguration<Follow>
{
    public void Configure(EntityTypeBuilder<Follow> builder)
    {
        builder.ToTable("follows");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.FollowerId).HasColumnName("follower_id").IsRequired();
        builder.Property(x => x.FollowingId).HasColumnName("following_id").IsRequired();
        builder.Property(x => x.TargetType).HasColumnName("target_type").IsRequired();
        builder.Property(x => x.NotificationsEnabled).HasColumnName("notifications_enabled").HasDefaultValue(true);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(x => new { x.FollowerId, x.FollowingId, x.TargetType }).IsUnique();
        builder.HasIndex(x => x.FollowerId);
        builder.HasIndex(x => new { x.FollowingId, x.TargetType });
    }
}
