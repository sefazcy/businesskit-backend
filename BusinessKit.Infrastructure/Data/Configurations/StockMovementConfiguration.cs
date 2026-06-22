using BusinessKit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessKit.Infrastructure.Data.Configurations;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.HasKey(sm => sm.Id);
        builder.ToTable("StockMovements");

        builder.Property(sm => sm.Type)
            .IsRequired()
            .HasMaxLength(20);

        // Quantity precision matches Product stock fields (18, 4).
        builder.Property(sm => sm.Quantity)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(sm => sm.PreviousStock)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(sm => sm.NewStock)
            .IsRequired()
            .HasPrecision(18, 4);

        builder.Property(sm => sm.Reason)
            .HasMaxLength(150);

        builder.Property(sm => sm.Notes)
            .HasMaxLength(1000);

        // Restrict delete: prevent accidental product deletion from wiping stock history.
        // To delete a product that has movements, the movements must be removed first.
        builder.HasOne(sm => sm.Product)
            .WithMany()
            .HasForeignKey(sm => sm.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(sm => sm.ProductId);
        builder.HasIndex(sm => sm.Type);
        builder.HasIndex(sm => sm.CreatedAt);
    }
}
