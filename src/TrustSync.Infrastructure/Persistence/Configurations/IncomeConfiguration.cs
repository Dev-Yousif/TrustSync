using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrustSync.Domain.Entities;

namespace TrustSync.Infrastructure.Persistence.Configurations;

public sealed class IncomeConfiguration : IEntityTypeConfiguration<Income>
{
    public void Configure(EntityTypeBuilder<Income> builder)
    {
        builder.ToTable("Incomes");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Description).HasMaxLength(500);
        builder.Property(i => i.Amount).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(i => i.CurrencyCode).IsRequired().HasMaxLength(3);
        builder.Property(i => i.Date).IsRequired();
        builder.Property(i => i.SourceType).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(i => i.PaymentStatus).HasConversion<string>().HasMaxLength(50);
        builder.Property(i => i.RecurrenceType).HasConversion<string>().HasMaxLength(50);
        builder.Property(i => i.Notes).HasMaxLength(2000);
        builder.Property(i => i.ConvertedAmount).HasColumnType("decimal(18,2)");
        builder.Property(i => i.ConvertedCurrencyCode).IsRequired().HasMaxLength(3).HasDefaultValue("USD");
        builder.Property(i => i.ExchangeRateUsed).HasColumnType("decimal(18,6)").HasDefaultValue(1m);

        builder.HasIndex(i => i.Date);
        builder.HasIndex(i => new { i.IsDeleted, i.Date });
        builder.HasIndex(i => i.CompanyClientId);
        builder.HasIndex(i => i.ProjectId);
        builder.HasIndex(i => i.SourceType);

        builder.HasOne(i => i.CompanyClient)
            .WithMany(c => c.Incomes)
            .HasForeignKey(i => i.CompanyClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Project)
            .WithMany(p => p.Incomes)
            .HasForeignKey(i => i.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Currency)
            .WithMany()
            .HasForeignKey(i => i.CurrencyCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
