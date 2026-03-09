using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrustSync.Domain.Entities;

namespace TrustSync.Infrastructure.Persistence.Configurations;

public sealed class SavingEntryConfiguration : IEntityTypeConfiguration<SavingEntry>
{
    public void Configure(EntityTypeBuilder<SavingEntry> builder)
    {
        builder.ToTable("SavingEntries");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Amount).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(e => e.ConvertedAmount).HasColumnType("decimal(18,2)");
        builder.Property(e => e.ConvertedCurrencyCode).IsRequired().HasMaxLength(3).HasDefaultValue("USD");
        builder.Property(e => e.ExchangeRateUsed).HasColumnType("decimal(18,6)").HasDefaultValue(1m);
        builder.Property(e => e.Notes).HasMaxLength(2000);

        builder.HasIndex(e => e.SavingGoalId);
    }
}
