using BusinessKit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessKit.Infrastructure.Data.Configurations;

public class GalleryItemConfiguration : IEntityTypeConfiguration<GalleryItem>
{
    public void Configure(EntityTypeBuilder<GalleryItem> builder)
    {
        builder.HasKey(g => g.Id);

        builder.Property(g => g.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(g => g.Description)
            .HasMaxLength(1000);

        builder.Property(g => g.ImageUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(g => g.Category)
            .HasMaxLength(100);

        builder.Property(g => g.IsActive)
            .HasDefaultValue(true);

        builder.Property(g => g.DisplayOrder)
            .HasDefaultValue(0);
    }
}
