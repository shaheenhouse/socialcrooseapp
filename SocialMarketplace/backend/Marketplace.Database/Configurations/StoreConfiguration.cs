using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Marketplace.Database.Entities;

namespace Marketplace.Database.Configurations;

public class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.ToTable("stores");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).HasMaxLength(255).IsRequired();
        builder.Property(s => s.Slug).HasMaxLength(255).IsRequired();
        builder.Property(s => s.ShortDescription).HasMaxLength(500);
        builder.Property(s => s.LogoUrl).HasMaxLength(500);
        builder.Property(s => s.BannerUrl).HasMaxLength(500);
        builder.Property(s => s.Email).HasMaxLength(255);
        builder.Property(s => s.Phone).HasMaxLength(20);
        builder.Property(s => s.Website).HasMaxLength(255);
        builder.Property(s => s.Address).HasMaxLength(500);
        builder.Property(s => s.City).HasMaxLength(100);
        builder.Property(s => s.State).HasMaxLength(100);
        builder.Property(s => s.Country).HasMaxLength(100);
        builder.Property(s => s.PostalCode).HasMaxLength(20);
        builder.Property(s => s.Latitude).HasPrecision(10, 8);
        builder.Property(s => s.Longitude).HasPrecision(11, 8);
        builder.Property(s => s.BusinessRegistrationNumber).HasMaxLength(100);
        builder.Property(s => s.TaxId).HasMaxLength(50);
        builder.Property(s => s.CommissionRate).HasPrecision(5, 2).HasDefaultValue(10m);
        builder.Property(s => s.Rating).HasPrecision(3, 2);
        builder.Property(s => s.TotalSales).HasPrecision(18, 2);

        builder.HasIndex(s => s.Slug).IsUnique();
        builder.HasIndex(s => s.OwnerId);
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.IsVerified);
        builder.HasIndex(s => s.IsFeatured);
        builder.HasIndex(s => s.Rating);

        builder.HasOne(s => s.Owner)
            .WithMany(u => u.Stores)
            .HasForeignKey(s => s.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class StoreEmployeeConfiguration : IEntityTypeConfiguration<StoreEmployee>
{
    public void Configure(EntityTypeBuilder<StoreEmployee> builder)
    {
        builder.ToTable("store_employees");

        builder.HasKey(se => se.Id);

        builder.Property(se => se.Title).HasMaxLength(100);
        builder.Property(se => se.Department).HasMaxLength(100);

        builder.HasIndex(se => new { se.StoreId, se.UserId }).IsUnique();

        builder.HasOne(se => se.Store)
            .WithMany(s => s.Employees)
            .HasForeignKey(se => se.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(se => se.User)
            .WithMany()
            .HasForeignKey(se => se.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(se => se.Role)
            .WithMany()
            .HasForeignKey(se => se.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
