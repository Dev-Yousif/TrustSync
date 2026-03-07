using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YousifAccounting.Domain.Entities;

namespace YousifAccounting.Infrastructure.Persistence.Configurations;

public class SavingGoalConfiguration : IEntityTypeConfiguration<SavingGoal>
{
    public void Configure(EntityTypeBuilder<SavingGoal> builder)
    {
        builder.ToTable("SavingGoals");

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Description)
            .HasMaxLength(2000);

        builder.Property(s => s.TargetAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(s => s.CurrencyCode)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(s => s.ConvertedTargetAmount).HasColumnType("decimal(18,2)");
        builder.Property(s => s.ConvertedCurrencyCode).IsRequired().HasMaxLength(3).HasDefaultValue("USD");
        builder.Property(s => s.ExchangeRateUsed).HasColumnType("decimal(18,6)").HasDefaultValue(1m);

        builder.Ignore(s => s.SavedAmount);
        builder.Ignore(s => s.ProgressPercentage);

        builder.HasMany(s => s.Entries)
            .WithOne(e => e.SavingGoal)
            .HasForeignKey(e => e.SavingGoalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Currency)
            .WithMany()
            .HasForeignKey(s => s.CurrencyCode);
    }
}
