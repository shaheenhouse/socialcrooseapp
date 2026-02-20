using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Marketplace.Database.Entities;

namespace Marketplace.Database.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.InvoiceNumber).HasMaxLength(50).IsRequired();
        builder.Property(i => i.ClientName).HasMaxLength(255).IsRequired();
        builder.Property(i => i.ClientEmail).HasMaxLength(255);
        builder.Property(i => i.ClientPhone).HasMaxLength(20);
        builder.Property(i => i.ClientAddress).HasMaxLength(500);
        builder.Property(i => i.Status).HasMaxLength(20).IsRequired();
        builder.Property(i => i.Subtotal).HasPrecision(18, 2);
        builder.Property(i => i.TaxRate).HasPrecision(5, 2);
        builder.Property(i => i.TaxAmount).HasPrecision(18, 2);
        builder.Property(i => i.DiscountAmount).HasPrecision(18, 2);
        builder.Property(i => i.Total).HasPrecision(18, 2);
        builder.Property(i => i.Currency).HasMaxLength(3);
        builder.Property(i => i.Notes).HasMaxLength(2000);
        builder.Property(i => i.Terms).HasMaxLength(2000);
        builder.Property(i => i.PublicToken).HasMaxLength(100);
        builder.Property(i => i.PaymentMethod).HasMaxLength(50);

        builder.HasIndex(i => i.UserId);
        builder.HasIndex(i => i.InvoiceNumber).IsUnique();
        builder.HasIndex(i => i.Status);
        builder.HasIndex(i => i.DueDate);
        builder.HasIndex(i => i.PublicToken).IsUnique();

        builder.HasMany(i => i.Items)
            .WithOne(item => item.Invoice)
            .HasForeignKey(item => item.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.ToTable("invoice_items");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Description).HasMaxLength(500).IsRequired();
        builder.Property(i => i.Quantity).HasPrecision(10, 2);
        builder.Property(i => i.Unit).HasMaxLength(20);
        builder.Property(i => i.UnitPrice).HasPrecision(18, 2);
        builder.Property(i => i.Total).HasPrecision(18, 2);

        builder.HasIndex(i => i.InvoiceId);
    }
}
