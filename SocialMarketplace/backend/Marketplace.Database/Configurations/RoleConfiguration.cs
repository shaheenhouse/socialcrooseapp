using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Marketplace.Database.Entities;

namespace Marketplace.Database.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name).HasMaxLength(100).IsRequired();
        builder.Property(r => r.DisplayName).HasMaxLength(150);
        builder.Property(r => r.Description).HasMaxLength(500);
        builder.Property(r => r.Scope).HasMaxLength(50);
        builder.Property(r => r.Color).HasMaxLength(20);
        builder.Property(r => r.Icon).HasMaxLength(100);

        builder.HasIndex(r => r.Name);
        builder.HasIndex(r => new { r.Scope, r.ScopeEntityId });
    }
}

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).HasMaxLength(100).IsRequired();
        builder.Property(p => p.DisplayName).HasMaxLength(150);
        builder.Property(p => p.Description).HasMaxLength(500);
        builder.Property(p => p.Module).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Action).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Resource).HasMaxLength(100);

        builder.HasIndex(p => p.Name).IsUnique();
        builder.HasIndex(p => new { p.Module, p.Action });
    }
}

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_roles");

        builder.HasKey(ur => ur.Id);

        builder.Property(ur => ur.Scope).HasMaxLength(50);
        builder.Property(ur => ur.Notes).HasMaxLength(500);

        builder.HasIndex(ur => new { ur.UserId, ur.RoleId, ur.ScopeEntityId }).IsUnique();

        builder.HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("role_permissions");

        builder.HasKey(rp => rp.Id);

        builder.HasIndex(rp => new { rp.RoleId, rp.PermissionId }).IsUnique();

        builder.HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
