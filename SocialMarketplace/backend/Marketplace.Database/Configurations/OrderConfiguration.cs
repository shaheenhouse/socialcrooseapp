using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Marketplace.Database.Entities;

namespace Marketplace.Database.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber).HasMaxLength(50).IsRequired();
        builder.Property(o => o.OrderType).HasMaxLength(20).HasDefaultValue("Product");
        builder.Property(o => o.Subtotal).HasPrecision(18, 2);
        builder.Property(o => o.DiscountAmount).HasPrecision(18, 2);
        builder.Property(o => o.DiscountCode).HasMaxLength(50);
        builder.Property(o => o.TaxAmount).HasPrecision(18, 2);
        builder.Property(o => o.ShippingAmount).HasPrecision(18, 2);
        builder.Property(o => o.ServiceFee).HasPrecision(18, 2);
        builder.Property(o => o.TotalAmount).HasPrecision(18, 2);
        builder.Property(o => o.Currency).HasMaxLength(3).HasDefaultValue("USD");
        builder.Property(o => o.PlatformCommission).HasPrecision(18, 2);
        builder.Property(o => o.SellerEarnings).HasPrecision(18, 2);
        builder.Property(o => o.ShippingName).HasMaxLength(255);
        builder.Property(o => o.ShippingAddress).HasMaxLength(500);
        builder.Property(o => o.ShippingCity).HasMaxLength(100);
        builder.Property(o => o.ShippingState).HasMaxLength(100);
        builder.Property(o => o.ShippingCountry).HasMaxLength(100);
        builder.Property(o => o.ShippingPostalCode).HasMaxLength(20);
        builder.Property(o => o.ShippingPhone).HasMaxLength(20);
        builder.Property(o => o.ShippingMethod).HasMaxLength(100);
        builder.Property(o => o.TrackingNumber).HasMaxLength(100);
        builder.Property(o => o.TrackingUrl).HasMaxLength(500);
        builder.Property(o => o.BillingName).HasMaxLength(255);
        builder.Property(o => o.BillingAddress).HasMaxLength(500);
        builder.Property(o => o.BillingCity).HasMaxLength(100);
        builder.Property(o => o.BillingState).HasMaxLength(100);
        builder.Property(o => o.BillingCountry).HasMaxLength(100);
        builder.Property(o => o.BillingPostalCode).HasMaxLength(20);
        builder.Property(o => o.GiftMessage).HasMaxLength(500);

        builder.HasIndex(o => o.OrderNumber).IsUnique();
        builder.HasIndex(o => o.BuyerId);
        builder.HasIndex(o => o.StoreId);
        builder.HasIndex(o => o.SellerId);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.CreatedAt);
        builder.HasIndex(o => new { o.BuyerId, o.Status });
        builder.HasIndex(o => new { o.StoreId, o.Status });

        builder.HasOne(o => o.Buyer)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.BuyerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Store)
            .WithMany(s => s.Orders)
            .HasForeignKey(o => o.StoreId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Seller)
            .WithMany()
            .HasForeignKey(o => o.SellerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items");

        builder.HasKey(oi => oi.Id);

        builder.Property(oi => oi.ItemType).HasMaxLength(20).HasDefaultValue("Product");
        builder.Property(oi => oi.Name).HasMaxLength(255).IsRequired();
        builder.Property(oi => oi.Sku).HasMaxLength(100);
        builder.Property(oi => oi.UnitPrice).HasPrecision(18, 2);
        builder.Property(oi => oi.DiscountAmount).HasPrecision(18, 2);
        builder.Property(oi => oi.TotalPrice).HasPrecision(18, 2);
        builder.Property(oi => oi.Currency).HasMaxLength(3).HasDefaultValue("USD");
        builder.Property(oi => oi.ImageUrl).HasMaxLength(500);
        builder.Property(oi => oi.RefundAmount).HasPrecision(18, 2);

        builder.HasIndex(oi => oi.OrderId);
        builder.HasIndex(oi => oi.ProductId);
        builder.HasIndex(oi => oi.ServiceId);

        builder.HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(oi => oi.Product)
            .WithMany(p => p.OrderItems)
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(oi => oi.ProductVariant)
            .WithMany()
            .HasForeignKey(oi => oi.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(oi => oi.Service)
            .WithMany()
            .HasForeignKey(oi => oi.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(oi => oi.ServicePackage)
            .WithMany()
            .HasForeignKey(oi => oi.ServicePackageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
