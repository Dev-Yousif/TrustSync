using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrustSync.Domain.Entities;

namespace TrustSync.Infrastructure.Persistence.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("UserProfiles");

        builder.Property(u => u.DisplayName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.PasswordSalt)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.HashAlgorithm)
            .HasMaxLength(50);

        builder.Property(u => u.DefaultCurrencyCode)
            .HasMaxLength(3);

        builder.HasOne(u => u.DefaultCurrency)
            .WithMany()
            .HasForeignKey(u => u.DefaultCurrencyCode);
    }
}
