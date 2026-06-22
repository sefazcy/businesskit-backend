using BusinessKit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessKit.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.ToTable("Products");

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.Sku)
            .HasMaxLength(80);

        builder.Property(p => p.Category)
            .HasMaxLength(100);

        builder.Property(p => p.Unit)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(p => p.CurrentStock)
            .IsRequired()
            .HasPrecision(18, 4)
            .HasDefaultValue(0m);

        builder.Property(p => p.MinStock)
            .IsRequired()
            .HasPrecision(18, 4)
            .HasDefaultValue(0m);

        builder.Property(p => p.CostPrice)
            .IsRequired()
            .HasPrecision(18, 2)
            .HasDefaultValue(0m);

        builder.Property(p => p.SalePrice)
            .IsRequired()
            .HasPrecision(18, 2)
            .HasDefaultValue(0m);

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.Notes)
            .HasMaxLength(1000);

        // SKU uniqueness is enforced at the DB level (SQLite treats each NULL as distinct,
        // so multiple products without a SKU are allowed).
        builder.HasIndex(p => p.Sku).IsUnique();
        builder.HasIndex(p => p.Name);
        builder.HasIndex(p => p.Category);
        builder.HasIndex(p => p.IsActive);
    }
}
