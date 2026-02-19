using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Marketplace.Database.Entities;

namespace Marketplace.Database.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email).HasMaxLength(255).IsRequired();
        builder.Property(u => u.Username).HasMaxLength(100).IsRequired();
        builder.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();
        builder.Property(u => u.FirstName).HasMaxLength(100);
        builder.Property(u => u.LastName).HasMaxLength(100);
        builder.Property(u => u.PhoneNumber).HasMaxLength(20);
        builder.Property(u => u.AvatarUrl).HasMaxLength(500);
        builder.Property(u => u.Bio).HasMaxLength(2000);
        builder.Property(u => u.PreferredLanguage).HasMaxLength(10).HasDefaultValue("en");
        builder.Property(u => u.TimeZone).HasMaxLength(50);
        builder.Property(u => u.Currency).HasMaxLength(3).HasDefaultValue("USD");
        builder.Property(u => u.Country).HasMaxLength(100);
        builder.Property(u => u.City).HasMaxLength(100);
        builder.Property(u => u.Address).HasMaxLength(500);
        builder.Property(u => u.PostalCode).HasMaxLength(20);
        builder.Property(u => u.ReputationScore).HasPrecision(10, 2);
        builder.Property(u => u.AverageRating).HasPrecision(3, 2);

        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.Username).IsUnique();
        builder.HasIndex(u => u.Status);
        builder.HasIndex(u => u.CreatedAt);
        builder.HasIndex(u => u.IsVerifiedSeller);

        builder.HasOne(u => u.Profile)
            .WithOne(p => p.User)
            .HasForeignKey<UserProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.Wallet)
            .WithOne(w => w.User)
            .HasForeignKey<Wallet>(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("user_profiles");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.CompanyName).HasMaxLength(255);
        builder.Property(p => p.CompanyRegistrationNumber).HasMaxLength(100);
        builder.Property(p => p.TaxId).HasMaxLength(50);
        builder.Property(p => p.Website).HasMaxLength(255);
        builder.Property(p => p.LinkedInUrl).HasMaxLength(255);
        builder.Property(p => p.GitHubUrl).HasMaxLength(255);
        builder.Property(p => p.PortfolioUrl).HasMaxLength(255);
        builder.Property(p => p.Headline).HasMaxLength(200);
        builder.Property(p => p.HourlyRate).HasPrecision(10, 2);
        builder.Property(p => p.TotalEarnings).HasPrecision(18, 2);

        builder.HasIndex(p => p.UserId).IsUnique();
        builder.HasIndex(p => p.AvailableForHire);
    }
}
