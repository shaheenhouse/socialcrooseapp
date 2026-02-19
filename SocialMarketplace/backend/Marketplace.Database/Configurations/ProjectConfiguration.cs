using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Marketplace.Database.Entities;

namespace Marketplace.Database.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("projects");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title).HasMaxLength(255).IsRequired();
        builder.Property(p => p.Slug).HasMaxLength(255).IsRequired();
        builder.Property(p => p.BudgetType).HasMaxLength(20).HasDefaultValue("Fixed");
        builder.Property(p => p.BudgetMin).HasPrecision(18, 2);
        builder.Property(p => p.BudgetMax).HasPrecision(18, 2);
        builder.Property(p => p.AgreedBudget).HasPrecision(18, 2);
        builder.Property(p => p.Currency).HasMaxLength(3).HasDefaultValue("USD");
        builder.Property(p => p.ExperienceLevel).HasMaxLength(20);
        builder.Property(p => p.ProjectType).HasMaxLength(20);
        builder.Property(p => p.Visibility).HasMaxLength(20).HasDefaultValue("Public");
        builder.Property(p => p.Rating).HasPrecision(3, 2);

        builder.HasIndex(p => p.Slug).IsUnique();
        builder.HasIndex(p => p.ClientId);
        builder.HasIndex(p => p.FreelancerId);
        builder.HasIndex(p => p.CategoryId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.CreatedAt);
        builder.HasIndex(p => p.IsUrgent);
        builder.HasIndex(p => p.IsFeatured);

        builder.HasOne(p => p.Client)
            .WithMany()
            .HasForeignKey(p => p.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Freelancer)
            .WithMany()
            .HasForeignKey(p => p.FreelancerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Category)
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ProjectBidConfiguration : IEntityTypeConfiguration<ProjectBid>
{
    public void Configure(EntityTypeBuilder<ProjectBid> builder)
    {
        builder.ToTable("project_bids");

        builder.HasKey(pb => pb.Id);

        builder.Property(pb => pb.Amount).HasPrecision(18, 2);
        builder.Property(pb => pb.Currency).HasMaxLength(3).HasDefaultValue("USD");
        builder.Property(pb => pb.ClientRating).HasPrecision(3, 2);

        builder.HasIndex(pb => pb.ProjectId);
        builder.HasIndex(pb => pb.FreelancerId);
        builder.HasIndex(pb => pb.Status);
        builder.HasIndex(pb => new { pb.ProjectId, pb.FreelancerId }).IsUnique();

        builder.HasOne(pb => pb.Project)
            .WithMany(p => p.Bids)
            .HasForeignKey(pb => pb.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pb => pb.Freelancer)
            .WithMany()
            .HasForeignKey(pb => pb.FreelancerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ProjectMilestoneConfiguration : IEntityTypeConfiguration<ProjectMilestone>
{
    public void Configure(EntityTypeBuilder<ProjectMilestone> builder)
    {
        builder.ToTable("project_milestones");

        builder.HasKey(pm => pm.Id);

        builder.Property(pm => pm.Title).HasMaxLength(255).IsRequired();
        builder.Property(pm => pm.Amount).HasPrecision(18, 2);
        builder.Property(pm => pm.Currency).HasMaxLength(3).HasDefaultValue("USD");

        builder.HasIndex(pm => pm.ProjectId);
        builder.HasIndex(pm => pm.Status);

        builder.HasOne(pm => pm.Project)
            .WithMany(p => p.Milestones)
            .HasForeignKey(pm => pm.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pm => pm.Escrow)
            .WithOne(e => e.Milestone)
            .HasForeignKey<ProjectMilestone>(pm => pm.EscrowId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
