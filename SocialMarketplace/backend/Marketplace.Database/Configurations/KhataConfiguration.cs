using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Marketplace.Database.Entities;

namespace Marketplace.Database.Configurations;

public class KhataPartyConfiguration : IEntityTypeConfiguration<KhataParty>
{
    public void Configure(EntityTypeBuilder<KhataParty> builder)
    {
        builder.ToTable("khata_parties");

        builder.HasKey(k => k.Id);

        builder.Property(k => k.PartyName).HasMaxLength(255).IsRequired();
        builder.Property(k => k.PartyPhone).HasMaxLength(20);
        builder.Property(k => k.PartyAddress).HasMaxLength(500);
        builder.Property(k => k.Type).HasMaxLength(20).IsRequired();
        builder.Property(k => k.OpeningBalance).HasPrecision(18, 2);
        builder.Property(k => k.TotalCredit).HasPrecision(18, 2);
        builder.Property(k => k.TotalDebit).HasPrecision(18, 2);
        builder.Property(k => k.Balance).HasPrecision(18, 2);
        builder.Property(k => k.Notes).HasMaxLength(2000);

        builder.HasIndex(k => k.UserId);
        builder.HasIndex(k => new { k.UserId, k.PartyName });
        builder.HasIndex(k => k.Type);
        builder.HasIndex(k => k.LastTransactionAt);

        builder.HasMany(k => k.Entries)
            .WithOne(e => e.KhataParty)
            .HasForeignKey(e => e.KhataPartyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class KhataEntryConfiguration : IEntityTypeConfiguration<KhataEntry>
{
    public void Configure(EntityTypeBuilder<KhataEntry> builder)
    {
        builder.ToTable("khata_entries");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(e => e.Type).HasMaxLength(10).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(500).IsRequired();
        builder.Property(e => e.RunningBalance).HasPrecision(18, 2);
        builder.Property(e => e.AttachmentUrl).HasMaxLength(500);
        builder.Property(e => e.ReferenceNumber).HasMaxLength(100);

        builder.HasIndex(e => e.KhataPartyId);
        builder.HasIndex(e => e.TransactionDate);
        builder.HasIndex(e => e.Type);
    }
}
