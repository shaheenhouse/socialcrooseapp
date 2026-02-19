using Marketplace.Database.Entities.Social;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Marketplace.Database.Configurations.Social;

public class SearchHistoryConfiguration : IEntityTypeConfiguration<SearchHistory>
{
    public void Configure(EntityTypeBuilder<SearchHistory> builder)
    {
        builder.ToTable("search_history");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.Query).HasColumnName("query").HasMaxLength(500).IsRequired();
        builder.Property(x => x.Type).HasColumnName("type").IsRequired();
        builder.Property(x => x.Filters).HasColumnName("filters").HasColumnType("jsonb");
        builder.Property(x => x.ResultCount).HasColumnName("result_count");
        builder.Property(x => x.ClickedResultId).HasColumnName("clicked_result_id");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.Query);
        builder.HasIndex(x => x.CreatedAt);
    }
}

public class TrendingSearchConfiguration : IEntityTypeConfiguration<TrendingSearch>
{
    public void Configure(EntityTypeBuilder<TrendingSearch> builder)
    {
        builder.ToTable("trending_searches");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Query).HasColumnName("query").HasMaxLength(500).IsRequired();
        builder.Property(x => x.Type).HasColumnName("type").IsRequired();
        builder.Property(x => x.SearchCount).HasColumnName("search_count");
        builder.Property(x => x.PeriodStart).HasColumnName("period_start").IsRequired();
        builder.Property(x => x.PeriodEnd).HasColumnName("period_end").IsRequired();
        builder.Property(x => x.Rank).HasColumnName("rank");

        builder.HasIndex(x => new { x.PeriodStart, x.PeriodEnd, x.Type });
        builder.HasIndex(x => x.Rank);
    }
}
