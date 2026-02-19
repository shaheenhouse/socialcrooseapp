using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Marketplace.Database.Entities;

namespace Marketplace.Database.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).HasMaxLength(255).IsRequired();
        builder.Property(p => p.Slug).HasMaxLength(255).IsRequired();
        builder.Property(p => p.ShortDescription).HasMaxLength(500);
        builder.Property(p => p.Sku).HasMaxLength(100);
        builder.Property(p => p.Barcode).HasMaxLength(100);
        builder.Property(p => p.Price).HasPrecision(18, 2).IsRequired();
        builder.Property(p => p.CompareAtPrice).HasPrecision(18, 2);
        builder.Property(p => p.CostPrice).HasPrecision(18, 2);
        builder.Property(p => p.Currency).HasMaxLength(3).HasDefaultValue("USD");
        builder.Property(p => p.Weight).HasPrecision(10, 2);
        builder.Property(p => p.WeightUnit).HasMaxLength(10);
        builder.Property(p => p.Length).HasPrecision(10, 2);
        builder.Property(p => p.Width).HasPrecision(10, 2);
        builder.Property(p => p.Height).HasPrecision(10, 2);
        builder.Property(p => p.DimensionUnit).HasMaxLength(10);
        builder.Property(p => p.Rating).HasPrecision(3, 2);
        builder.Property(p => p.SeoTitle).HasMaxLength(255);
        builder.Property(p => p.SeoDescription).HasMaxLength(500);
        builder.Property(p => p.SeoKeywords).HasMaxLength(500);

        builder.HasIndex(p => p.Slug).IsUnique();
        builder.HasIndex(p => p.StoreId);
        builder.HasIndex(p => p.CategoryId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.Price);
        builder.HasIndex(p => p.Rating);
        builder.HasIndex(p => p.IsFeatured);
        builder.HasIndex(p => p.Sku);
        builder.HasIndex(p => new { p.StoreId, p.Status });

        builder.HasOne(p => p.Store)
            .WithMany(s => s.Products)
            .HasForeignKey(p => p.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("product_images");

        builder.HasKey(pi => pi.Id);

        builder.Property(pi => pi.Url).HasMaxLength(500).IsRequired();
        builder.Property(pi => pi.ThumbnailUrl).HasMaxLength(500);
        builder.Property(pi => pi.AltText).HasMaxLength(255);
        builder.Property(pi => pi.MimeType).HasMaxLength(50);

        builder.HasIndex(pi => pi.ProductId);
        builder.HasIndex(pi => new { pi.ProductId, pi.IsPrimary });

        builder.HasOne(pi => pi.Product)
            .WithMany(p => p.Images)
            .HasForeignKey(pi => pi.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("product_variants");

        builder.HasKey(pv => pv.Id);

        builder.Property(pv => pv.Name).HasMaxLength(255).IsRequired();
        builder.Property(pv => pv.Sku).HasMaxLength(100);
        builder.Property(pv => pv.Barcode).HasMaxLength(100);
        builder.Property(pv => pv.Price).HasPrecision(18, 2).IsRequired();
        builder.Property(pv => pv.CompareAtPrice).HasPrecision(18, 2);
        builder.Property(pv => pv.CostPrice).HasPrecision(18, 2);
        builder.Property(pv => pv.ImageUrl).HasMaxLength(500);
        builder.Property(pv => pv.Weight).HasPrecision(10, 2);

        builder.HasIndex(pv => pv.ProductId);
        builder.HasIndex(pv => pv.Sku);

        builder.HasOne(pv => pv.Product)
            .WithMany(p => p.Variants)
            .HasForeignKey(pv => pv.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
