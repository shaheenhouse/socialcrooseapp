using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Marketplace.Database.Entities;

namespace Marketplace.Database.Configurations;

public class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> builder)
    {
        builder.ToTable("skills");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).HasMaxLength(100).IsRequired();
        builder.Property(s => s.Slug).HasMaxLength(100).IsRequired();
        builder.Property(s => s.Description).HasMaxLength(500);
        builder.Property(s => s.IconUrl).HasMaxLength(500);

        builder.HasIndex(s => s.Name).IsUnique();
        builder.HasIndex(s => s.Slug).IsUnique();
        builder.HasIndex(s => s.CategoryId);
        builder.HasIndex(s => s.IsActive);
        builder.HasIndex(s => s.IsFeatured);

        builder.HasOne(s => s.Category)
            .WithMany(c => c.Skills)
            .HasForeignKey(s => s.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class UserSkillConfiguration : IEntityTypeConfiguration<UserSkill>
{
    public void Configure(EntityTypeBuilder<UserSkill> builder)
    {
        builder.ToTable("user_skills");

        builder.HasKey(us => us.Id);

        builder.Property(us => us.Description).HasMaxLength(500);
        builder.Property(us => us.PortfolioUrl).HasMaxLength(500);

        builder.HasIndex(us => new { us.UserId, us.SkillId }).IsUnique();
        builder.HasIndex(us => us.VerificationStatus);
        builder.HasIndex(us => us.Level);

        builder.HasOne(us => us.User)
            .WithMany(u => u.Skills)
            .HasForeignKey(us => us.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(us => us.Skill)
            .WithMany(s => s.UserSkills)
            .HasForeignKey(us => us.SkillId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SkillTestConfiguration : IEntityTypeConfiguration<SkillTest>
{
    public void Configure(EntityTypeBuilder<SkillTest> builder)
    {
        builder.ToTable("skill_tests");

        builder.HasKey(st => st.Id);

        builder.Property(st => st.Title).HasMaxLength(255).IsRequired();
        builder.Property(st => st.Description).HasMaxLength(1000);

        builder.HasIndex(st => st.SkillId);
        builder.HasIndex(st => st.IsActive);

        builder.HasOne(st => st.Skill)
            .WithMany(s => s.Tests)
            .HasForeignKey(st => st.SkillId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SkillTestQuestionConfiguration : IEntityTypeConfiguration<SkillTestQuestion>
{
    public void Configure(EntityTypeBuilder<SkillTestQuestion> builder)
    {
        builder.ToTable("skill_test_questions");

        builder.HasKey(stq => stq.Id);

        builder.Property(stq => stq.Question).IsRequired();
        builder.Property(stq => stq.QuestionType).HasMaxLength(50).HasDefaultValue("MultipleChoice");
        builder.Property(stq => stq.CorrectAnswer).IsRequired();
        builder.Property(stq => stq.Difficulty).HasMaxLength(20);
        builder.Property(stq => stq.CodeLanguage).HasMaxLength(50);

        builder.HasIndex(stq => stq.TestId);
        builder.HasIndex(stq => stq.IsActive);

        builder.HasOne(stq => stq.Test)
            .WithMany(st => st.Questions)
            .HasForeignKey(stq => stq.TestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SkillTestAttemptConfiguration : IEntityTypeConfiguration<SkillTestAttempt>
{
    public void Configure(EntityTypeBuilder<SkillTestAttempt> builder)
    {
        builder.ToTable("skill_test_attempts");

        builder.HasKey(sta => sta.Id);

        builder.Property(sta => sta.Percentage).HasPrecision(5, 2);
        builder.Property(sta => sta.IpAddress).HasMaxLength(45);

        builder.HasIndex(sta => sta.UserId);
        builder.HasIndex(sta => sta.TestId);
        builder.HasIndex(sta => sta.Status);
        builder.HasIndex(sta => new { sta.UserId, sta.TestId, sta.StartedAt });

        builder.HasOne(sta => sta.User)
            .WithMany()
            .HasForeignKey(sta => sta.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sta => sta.Test)
            .WithMany(st => st.Attempts)
            .HasForeignKey(sta => sta.TestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
