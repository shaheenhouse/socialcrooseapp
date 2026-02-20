using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Marketplace.Database.Entities;

namespace Marketplace.Database.Configurations;

public class ResumeConfiguration : IEntityTypeConfiguration<Resume>
{
    public void Configure(EntityTypeBuilder<Resume> builder)
    {
        builder.ToTable("resumes");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Title).HasMaxLength(255).HasDefaultValue("My Resume");
        builder.Property(r => r.Template).HasMaxLength(50).HasDefaultValue("modern");
        builder.Property(r => r.IsPublic).HasDefaultValue(false);
        builder.Property(r => r.PdfUrl).HasMaxLength(500);

        builder.Property(r => r.PersonalInfo).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");
        builder.Property(r => r.Education).HasColumnType("jsonb").HasDefaultValueSql("'[]'::jsonb");
        builder.Property(r => r.Experience).HasColumnType("jsonb").HasDefaultValueSql("'[]'::jsonb");
        builder.Property(r => r.Skills).HasColumnType("jsonb").HasDefaultValueSql("'[]'::jsonb");
        builder.Property(r => r.Certifications).HasColumnType("jsonb").HasDefaultValueSql("'[]'::jsonb");
        builder.Property(r => r.Projects).HasColumnType("jsonb").HasDefaultValueSql("'[]'::jsonb");
        builder.Property(r => r.Languages).HasColumnType("jsonb").HasDefaultValueSql("'[]'::jsonb");
        builder.Property(r => r.CustomSections).HasColumnType("jsonb").HasDefaultValueSql("'[]'::jsonb");

        builder.HasIndex(r => r.UserId);

        builder.HasOne(r => r.User)
            .WithMany(u => u.Resumes)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
