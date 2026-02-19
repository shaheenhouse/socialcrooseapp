using Marketplace.Database.Entities.Social;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marketplace.Database.Configurations.Social;

public class PageConfiguration : IEntityTypeConfiguration<Page>
{
    public void Configure(EntityTypeBuilder<Page> builder)
    {
        builder.ToTable("pages");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Slug).HasColumnName("slug").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(5000);
        builder.Property(x => x.Tagline).HasColumnName("tagline").HasMaxLength(500);
        builder.Property(x => x.LogoUrl).HasColumnName("logo_url").HasMaxLength(500);
        builder.Property(x => x.CoverImageUrl).HasColumnName("cover_image_url").HasMaxLength(500);
        builder.Property(x => x.Website).HasColumnName("website").HasMaxLength(500);
        builder.Property(x => x.Industry).HasColumnName("industry").HasMaxLength(200);
        builder.Property(x => x.CompanySize).HasColumnName("company_size").HasMaxLength(50);
        builder.Property(x => x.Headquarters).HasColumnName("headquarters").HasMaxLength(200);
        builder.Property(x => x.FoundedYear).HasColumnName("founded_year");
        builder.Property(x => x.Type).HasColumnName("type").IsRequired();
        builder.Property(x => x.OwnerId).HasColumnName("owner_id").IsRequired();
        builder.Property(x => x.IsVerified).HasColumnName("is_verified").HasDefaultValue(false);
        builder.Property(x => x.FollowerCount).HasColumnName("follower_count").HasDefaultValue(0);
        builder.Property(x => x.EmployeeCount).HasColumnName("employee_count").HasDefaultValue(0);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsRequired();
        builder.Property(x => x.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(x => x.LinkedInUrl).HasColumnName("linkedin_url").HasMaxLength(500);
        builder.Property(x => x.TwitterUrl).HasColumnName("twitter_url").HasMaxLength(500);
        builder.Property(x => x.FacebookUrl).HasColumnName("facebook_url").HasMaxLength(500);
        builder.Property(x => x.InstagramUrl).HasColumnName("instagram_url").HasMaxLength(500);

        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => x.OwnerId);
        builder.HasIndex(x => x.Type);
        builder.HasIndex(x => x.IsActive);
    }
}
