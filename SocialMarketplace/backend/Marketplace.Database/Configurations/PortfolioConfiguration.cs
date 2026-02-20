using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Marketplace.Database.Entities;

namespace Marketplace.Database.Configurations;

public class PortfolioConfiguration : IEntityTypeConfiguration<Portfolio>
{
    public void Configure(EntityTypeBuilder<Portfolio> builder)
    {
        builder.ToTable("portfolios");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Slug).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Theme).HasMaxLength(50).HasDefaultValue("dark");
        builder.Property(p => p.IsPublic).HasDefaultValue(false);

        builder.Property(p => p.PersonalInfo).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
        builder.Property(p => p.Education).HasColumnType("jsonb").HasDefaultValueSql("'[]'::jsonb");
        builder.Property(p => p.Experience).HasColumnType("jsonb").HasDefaultValueSql("'[]'::jsonb");
        builder.Property(p => p.Skills).HasColumnType("jsonb").HasDefaultValueSql("'[]'::jsonb");
        builder.Property(p => p.Roles).HasColumnType("jsonb").HasDefaultValueSql("'[]'::jsonb");
        builder.Property(p => p.Certifications).HasColumnType("jsonb").HasDefaultValueSql("'[]'::jsonb");
        builder.Property(p => p.Projects).HasColumnType("jsonb").HasDefaultValueSql("'[]'::jsonb");
        builder.Property(p => p.Achievements).HasColumnType("jsonb").HasDefaultValueSql("'[]'::jsonb");
        builder.Property(p => p.Languages).HasColumnType("jsonb").HasDefaultValueSql("'[]'::jsonb");
        builder.Property(p => p.Resumes).HasColumnType("jsonb").HasDefaultValueSql("'[]'::jsonb");

        builder.HasIndex(p => p.UserId).IsUnique();
        builder.HasIndex(p => p.Slug).IsUnique();
        builder.HasIndex(p => p.IsPublic);

        builder.HasOne(p => p.User)
            .WithOne(u => u.Portfolio)
            .HasForeignKey<Portfolio>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
