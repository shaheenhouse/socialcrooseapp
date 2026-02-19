using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Marketplace.Database.Entities;

namespace Marketplace.Database.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.PaymentNumber).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Amount).HasPrecision(18, 2);
        builder.Property(p => p.Currency).HasMaxLength(3).HasDefaultValue("USD");
        builder.Property(p => p.PaymentMethod).HasMaxLength(50).IsRequired();
        builder.Property(p => p.PaymentGateway).HasMaxLength(50);
        builder.Property(p => p.GatewayTransactionId).HasMaxLength(255);
        builder.Property(p => p.PaymentIntentId).HasMaxLength(255);
        builder.Property(p => p.ChargeId).HasMaxLength(255);
        builder.Property(p => p.GatewayFee).HasPrecision(18, 2);
        builder.Property(p => p.PlatformFee).HasPrecision(18, 2);
        builder.Property(p => p.NetAmount).HasPrecision(18, 2);
        builder.Property(p => p.Description).HasMaxLength(500);
        builder.Property(p => p.FailureReason).HasMaxLength(500);
        builder.Property(p => p.FailureCode).HasMaxLength(50);
        builder.Property(p => p.RefundReason).HasMaxLength(500);
        builder.Property(p => p.IpAddress).HasMaxLength(45);
        builder.Property(p => p.CardBrand).HasMaxLength(20);
        builder.Property(p => p.CardLast4).HasMaxLength(4);
        builder.Property(p => p.ReceiptUrl).HasMaxLength(500);
        builder.Property(p => p.InvoiceUrl).HasMaxLength(500);

        builder.HasIndex(p => p.PaymentNumber).IsUnique();
        builder.HasIndex(p => p.OrderId);
        builder.HasIndex(p => p.PayerId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.CreatedAt);
        builder.HasIndex(p => p.GatewayTransactionId);

        builder.HasOne(p => p.Order)
            .WithMany(o => o.Payments)
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Payer)
            .WithMany()
            .HasForeignKey(p => p.PayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Payee)
            .WithMany()
            .HasForeignKey(p => p.PayeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class EscrowConfiguration : IEntityTypeConfiguration<Escrow>
{
    public void Configure(EntityTypeBuilder<Escrow> builder)
    {
        builder.ToTable("escrows");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EscrowNumber).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Amount).HasPrecision(18, 2);
        builder.Property(e => e.ReleasedAmount).HasPrecision(18, 2);
        builder.Property(e => e.RefundedAmount).HasPrecision(18, 2);
        builder.Property(e => e.HeldAmount).HasPrecision(18, 2);
        builder.Property(e => e.Currency).HasMaxLength(3).HasDefaultValue("USD");

        builder.HasIndex(e => e.EscrowNumber).IsUnique();
        builder.HasIndex(e => e.OrderId);
        builder.HasIndex(e => e.ProjectId);
        builder.HasIndex(e => e.BuyerId);
        builder.HasIndex(e => e.SellerId);
        builder.HasIndex(e => e.Status);

        builder.HasOne(e => e.Order)
            .WithOne(o => o.Escrow)
            .HasForeignKey<Escrow>(e => e.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Buyer)
            .WithMany()
            .HasForeignKey(e => e.BuyerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Seller)
            .WithMany()
            .HasForeignKey(e => e.SellerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.ToTable("wallets");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Balance).HasPrecision(18, 2);
        builder.Property(w => w.PendingBalance).HasPrecision(18, 2);
        builder.Property(w => w.HeldBalance).HasPrecision(18, 2);
        builder.Property(w => w.Currency).HasMaxLength(3).HasDefaultValue("USD");
        builder.Property(w => w.TotalEarned).HasPrecision(18, 2);
        builder.Property(w => w.TotalWithdrawn).HasPrecision(18, 2);
        builder.Property(w => w.TotalSpent).HasPrecision(18, 2);
        builder.Property(w => w.LockReason).HasMaxLength(500);

        builder.HasIndex(w => w.UserId).IsUnique();
    }
}

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.TransactionNumber).HasMaxLength(50).IsRequired();
        builder.Property(t => t.Amount).HasPrecision(18, 2);
        builder.Property(t => t.BalanceBefore).HasPrecision(18, 2);
        builder.Property(t => t.BalanceAfter).HasPrecision(18, 2);
        builder.Property(t => t.Currency).HasMaxLength(3).HasDefaultValue("USD");
        builder.Property(t => t.Description).HasMaxLength(500);
        builder.Property(t => t.Reference).HasMaxLength(255);
        builder.Property(t => t.ReferenceType).HasMaxLength(50);

        builder.HasIndex(t => t.TransactionNumber).IsUnique();
        builder.HasIndex(t => t.WalletId);
        builder.HasIndex(t => t.Type);
        builder.HasIndex(t => t.CreatedAt);
        builder.HasIndex(t => new { t.WalletId, t.CreatedAt });

        builder.HasOne(t => t.Wallet)
            .WithMany(w => w.Transactions)
            .HasForeignKey(t => t.WalletId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
