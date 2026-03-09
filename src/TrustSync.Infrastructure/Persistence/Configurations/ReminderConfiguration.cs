using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrustSync.Domain.Entities;

namespace TrustSync.Infrastructure.Persistence.Configurations;

public sealed class ReminderConfiguration : IEntityTypeConfiguration<Reminder>
{
    public void Configure(EntityTypeBuilder<Reminder> builder)
    {
        builder.ToTable("Reminders");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Title).IsRequired().HasMaxLength(200);
        builder.Property(r => r.Description).HasMaxLength(500);
        builder.Property(r => r.RepeatType).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(r => r.TimeOfDay).HasConversion(
            v => v.ToString("HH:mm"),
            v => TimeOnly.Parse(v)).HasMaxLength(5);

        builder.HasIndex(r => r.IsEnabled);
        builder.HasIndex(r => r.NextFireAt);
    }
}
