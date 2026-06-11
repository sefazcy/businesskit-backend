using BusinessKit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessKit.Infrastructure.Data.Configurations;

public class BusinessServiceConfiguration : IEntityTypeConfiguration<BusinessService>
{
    public void Configure(EntityTypeBuilder<BusinessService> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Slug)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(s => s.Slug)
            .IsUnique();

        builder.Property(s => s.ShortDescription)
            .HasMaxLength(500);

        builder.Property(s => s.Price)
            .HasPrecision(18, 2);

        builder.Property(s => s.IsActive)
            .HasDefaultValue(true);

        builder.Property(s => s.DisplayOrder)
            .HasDefaultValue(0);
    }
}