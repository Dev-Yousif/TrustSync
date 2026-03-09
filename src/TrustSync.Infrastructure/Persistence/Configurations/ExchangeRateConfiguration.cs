using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrustSync.Domain.Entities;

namespace TrustSync.Infrastructure.Persistence.Configurations;

public sealed class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(EntityTypeBuilder<ExchangeRate> builder)
    {
        builder.ToTable("ExchangeRates");
        builder.HasKey(e => e.CurrencyCode);

        builder.Property(e => e.CurrencyCode).HasMaxLength(3);
        builder.Property(e => e.RateToUsd).IsRequired().HasColumnType("decimal(18,6)");
        builder.Property(e => e.FetchedAt).IsRequired();
    }
}
