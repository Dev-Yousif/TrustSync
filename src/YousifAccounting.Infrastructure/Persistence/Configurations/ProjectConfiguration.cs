using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YousifAccounting.Domain.Entities;

namespace YousifAccounting.Infrastructure.Persistence.Configurations;

public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Description).HasMaxLength(2000);
        builder.Property(p => p.Status).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(p => p.AgreedAmount).HasColumnType("decimal(18,2)");
        builder.Property(p => p.ReceivedAmount).HasColumnType("decimal(18,2)");
        builder.Property(p => p.ExpectedAmount).HasColumnType("decimal(18,2)");
        builder.Property(p => p.CurrencyCode).IsRequired().HasMaxLength(3);
        builder.Property(p => p.Notes).HasMaxLength(2000);

        builder.HasIndex(p => p.CompanyClientId);
        builder.HasIndex(p => p.Status);

        builder.HasOne(p => p.CompanyClient)
            .WithMany(c => c.Projects)
            .HasForeignKey(p => p.CompanyClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Currency)
            .WithMany()
            .HasForeignKey(p => p.CurrencyCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
