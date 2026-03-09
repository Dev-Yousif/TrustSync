using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrustSync.Domain.Entities;

namespace TrustSync.Infrastructure.Persistence.Configurations;

public sealed class BackupRecordConfiguration : IEntityTypeConfiguration<BackupRecord>
{
    public void Configure(EntityTypeBuilder<BackupRecord> builder)
    {
        builder.ToTable("BackupRecords");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.FileName).IsRequired().HasMaxLength(300);
        builder.Property(b => b.FilePath).IsRequired().HasMaxLength(1000);
        builder.Property(b => b.Type).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(b => b.Checksum).HasMaxLength(128);
        builder.Property(b => b.Notes).HasMaxLength(2000);

        builder.HasIndex(b => b.CreatedAt).IsDescending();
    }
}
