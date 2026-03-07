using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YousifAccounting.Domain.Entities;

namespace YousifAccounting.Infrastructure.Persistence.Configurations;

public sealed class SavingEntryConfiguration : IEntityTypeConfiguration<SavingEntry>
{
    public void Configure(EntityTypeBuilder<SavingEntry> builder)
    {
        builder.ToTable("SavingEntries");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Amount).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(e => e.Notes).HasMaxLength(2000);

        builder.HasIndex(e => e.SavingGoalId);
    }
}
