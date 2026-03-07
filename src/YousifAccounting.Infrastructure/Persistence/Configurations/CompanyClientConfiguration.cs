using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YousifAccounting.Domain.Entities;

namespace YousifAccounting.Infrastructure.Persistence.Configurations;

public sealed class CompanyClientConfiguration : IEntityTypeConfiguration<CompanyClient>
{
    public void Configure(EntityTypeBuilder<CompanyClient> builder)
    {
        builder.ToTable("CompanyClients");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Type).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(c => c.EngagementType).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(c => c.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.ContactEmail).HasMaxLength(200);
        builder.Property(c => c.ContactPhone).HasMaxLength(200);
        builder.Property(c => c.Website).HasMaxLength(200);
        builder.Property(c => c.Notes).HasMaxLength(2000);
        builder.Property(c => c.DefaultCurrencyCode).IsRequired().HasMaxLength(3);

        builder.HasIndex(c => c.Name);
        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.IsDeleted);

        builder.HasOne(c => c.DefaultCurrency)
            .WithMany()
            .HasForeignKey(c => c.DefaultCurrencyCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
