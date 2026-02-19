using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Marketplace.Database.Entities;

namespace Marketplace.Database.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("reviews");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.ReviewType).HasMaxLength(20).IsRequired();
        builder.Property(r => r.Title).HasMaxLength(255);
        builder.Property(r => r.Content).HasMaxLength(5000);
        builder.Property(r => r.HiddenReason).HasMaxLength(500);

        builder.HasIndex(r => r.ReviewerId);
        builder.HasIndex(r => r.RevieweeId);
        builder.HasIndex(r => r.OrderId);
        builder.HasIndex(r => r.ProductId);
        builder.HasIndex(r => r.ServiceId);
        builder.HasIndex(r => r.StoreId);
        builder.HasIndex(r => r.ProjectId);
        builder.HasIndex(r => r.Rating);
        builder.HasIndex(r => r.CreatedAt);

        builder.HasOne(r => r.Reviewer)
            .WithMany(u => u.ReviewsGiven)
            .HasForeignKey(r => r.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Reviewee)
            .WithMany(u => u.ReviewsReceived)
            .HasForeignKey(r => r.RevieweeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Order)
            .WithMany(o => o.Reviews)
            .HasForeignKey(r => r.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Product)
            .WithMany(p => p.Reviews)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Service)
            .WithMany(s => s.Reviews)
            .HasForeignKey(r => r.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Store)
            .WithMany(s => s.Reviews)
            .HasForeignKey(r => r.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Project)
            .WithMany(p => p.Reviews)
            .HasForeignKey(r => r.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ReviewResponseConfiguration : IEntityTypeConfiguration<ReviewResponse>
{
    public void Configure(EntityTypeBuilder<ReviewResponse> builder)
    {
        builder.ToTable("review_responses");

        builder.HasKey(rr => rr.Id);

        builder.Property(rr => rr.Content).HasMaxLength(2000).IsRequired();
        builder.Property(rr => rr.HiddenReason).HasMaxLength(500);

        builder.HasIndex(rr => rr.ReviewId).IsUnique();

        builder.HasOne(rr => rr.Review)
            .WithOne(r => r.Response)
            .HasForeignKey<ReviewResponse>(rr => rr.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rr => rr.Responder)
            .WithMany()
            .HasForeignKey(rr => rr.ResponderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
