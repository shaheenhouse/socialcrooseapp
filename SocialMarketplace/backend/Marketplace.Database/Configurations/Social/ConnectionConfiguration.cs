using Marketplace.Database.Entities.Social;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marketplace.Database.Configurations.Social;

public class ConnectionConfiguration : IEntityTypeConfiguration<Connection>
{
    public void Configure(EntityTypeBuilder<Connection> builder)
    {
        builder.ToTable("connections");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.RequesterId).HasColumnName("requester_id").IsRequired();
        builder.Property(x => x.AddresseeId).HasColumnName("addressee_id").IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").IsRequired();
        builder.Property(x => x.Message).HasColumnName("message").HasMaxLength(500);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.AcceptedAt).HasColumnName("accepted_at");
        builder.Property(x => x.RejectedAt).HasColumnName("rejected_at");
        builder.Property(x => x.BlockedAt).HasColumnName("blocked_at");

        builder.HasIndex(x => new { x.RequesterId, x.AddresseeId }).IsUnique();
        builder.HasIndex(x => x.RequesterId);
        builder.HasIndex(x => x.AddresseeId);
        builder.HasIndex(x => x.Status);
    }
}
