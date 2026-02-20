using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Marketplace.Database.Entities;

namespace Marketplace.Database.Configurations;

public class DesignConfiguration : IEntityTypeConfiguration<Design>
{
    public void Configure(EntityTypeBuilder<Design> builder)
    {
        builder.ToTable("designs");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name).HasMaxLength(255).HasDefaultValue("Untitled Design");
        builder.Property(d => d.Description).HasMaxLength(1000);
        builder.Property(d => d.Width).HasDefaultValue(1080);
        builder.Property(d => d.Height).HasDefaultValue(1080);
        builder.Property(d => d.CanvasJson).HasColumnType("text").HasDefaultValue("{}");
        builder.Property(d => d.Thumbnail).HasColumnType("text").HasDefaultValue("");
        builder.Property(d => d.Status).HasDefaultValue(Enums.DesignStatus.Draft)
            .HasConversion<string>().HasMaxLength(20);
        builder.Property(d => d.Category).HasMaxLength(50).HasDefaultValue("custom");
        builder.Property(d => d.Tags).HasColumnType("jsonb").HasDefaultValueSql("'[]'::jsonb");
        builder.Property(d => d.IsTemplate).HasDefaultValue(false);
        builder.Property(d => d.IsPublic).HasDefaultValue(false);

        builder.HasIndex(d => d.UserId);
        builder.HasIndex(d => d.Status);
        builder.HasIndex(d => d.Category);
        builder.HasIndex(d => d.IsTemplate);
        builder.HasIndex(d => d.IsPublic);

        builder.HasOne(d => d.User)
            .WithMany(u => u.Designs)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
