using BusinessKit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessKit.Infrastructure.Data.Configurations;

public class ApartmentUnitConfiguration : IEntityTypeConfiguration<ApartmentUnit>
{
    public void Configure(EntityTypeBuilder<ApartmentUnit> builder)
    {
        builder.HasKey(u => u.Id);
        builder.ToTable("ApartmentUnits");

        builder.Property(u => u.BlockName)
            .HasMaxLength(50);

        builder.Property(u => u.DoorNumber)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(u => u.UnitType)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(u => u.GrossArea)
            .HasPrecision(18, 2);

        builder.Property(u => u.NetArea)
            .HasPrecision(18, 2);

        builder.Property(u => u.IsOccupied)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.Notes)
            .HasMaxLength(1000);

        // Service-level uniqueness check in EnsureUnitUniqueAsync handles all cases including
        // null BlockName (SQLite treats each NULL as distinct in unique indexes, so the DB
        // constraint alone would allow duplicate null-block + same DoorNumber combinations).
        builder.HasIndex(u => new { u.BlockName, u.DoorNumber }).IsUnique();
        builder.HasIndex(u => u.IsActive);
        builder.HasIndex(u => u.IsOccupied);
    }
}
