using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrustSync.Domain.Entities;

namespace TrustSync.Infrastructure.Persistence.Configurations;

public sealed class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("Expenses");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.Amount).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(e => e.CurrencyCode).IsRequired().HasMaxLength(3);
        builder.Property(e => e.Date).IsRequired();
        builder.Property(e => e.ExpenseType).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.RecurrenceType).HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.Notes).HasMaxLength(2000);
        builder.Property(e => e.AttachmentPath).HasMaxLength(500);
        builder.Property(e => e.ConvertedAmount).HasColumnType("decimal(18,2)");
        builder.Property(e => e.ConvertedCurrencyCode).IsRequired().HasMaxLength(3).HasDefaultValue("USD");
        builder.Property(e => e.ExchangeRateUsed).HasColumnType("decimal(18,6)").HasDefaultValue(1m);

        builder.HasIndex(e => e.Date);
        builder.HasIndex(e => new { e.IsDeleted, e.Date });
        builder.HasIndex(e => e.CategoryId);
        builder.HasIndex(e => e.CompanyClientId);
        builder.HasIndex(e => e.ProjectId);

        builder.HasOne(e => e.Category)
            .WithMany(c => c.Expenses)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.CompanyClient)
            .WithMany(c => c.Expenses)
            .HasForeignKey(e => e.CompanyClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Project)
            .WithMany(p => p.Expenses)
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Currency)
            .WithMany()
            .HasForeignKey(e => e.CurrencyCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
