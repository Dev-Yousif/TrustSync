using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YousifAccounting.Domain.Entities;

namespace YousifAccounting.Infrastructure.Persistence.Configurations;

public class DeductionConfiguration : IEntityTypeConfiguration<Deduction>
{
    public void Configure(EntityTypeBuilder<Deduction> builder)
    {
        builder.ToTable("Deductions");

        builder.Property(d => d.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.Description)
            .HasMaxLength(2000);

        builder.Property(d => d.Amount)
            .HasColumnType("decimal(18,2)");

        builder.Property(d => d.CurrencyCode)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(d => d.Type)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(d => d.RecurrenceType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(d => d.Notes)
            .HasMaxLength(2000);

        builder.Property(d => d.ConvertedAmount).HasColumnType("decimal(18,2)");
        builder.Property(d => d.ConvertedCurrencyCode).IsRequired().HasMaxLength(3).HasDefaultValue("USD");
        builder.Property(d => d.ExchangeRateUsed).HasColumnType("decimal(18,6)").HasDefaultValue(1m);

        builder.HasIndex(d => new { d.IsActive, d.Type });

        builder.HasOne(d => d.Currency)
            .WithMany()
            .HasForeignKey(d => d.CurrencyCode);
    }
}
