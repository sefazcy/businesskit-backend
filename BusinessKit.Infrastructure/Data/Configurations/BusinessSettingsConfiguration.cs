using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BusinessSettingsEntity = BusinessKit.Domain.Entities.BusinessSettings;

namespace BusinessKit.Infrastructure.Data.Configurations;

public class BusinessSettingsConfiguration : IEntityTypeConfiguration<BusinessSettingsEntity>
{
    public void Configure(EntityTypeBuilder<BusinessSettingsEntity> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.BusinessName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.LogoUrl)
            .HasMaxLength(500);

        builder.Property(b => b.Phone)
            .HasMaxLength(30);

        builder.Property(b => b.Email)
            .HasMaxLength(200);

        builder.Property(b => b.Address)
            .HasMaxLength(500);

        builder.Property(b => b.WhatsApp)
            .HasMaxLength(30);

        builder.Property(b => b.LinkedInUrl)
            .HasMaxLength(500);

        builder.Property(b => b.Currency)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("USD");

        builder.Property(b => b.ThemeColor)
            .HasMaxLength(20);
    }
}
