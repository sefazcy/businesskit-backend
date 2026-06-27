using BusinessKit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessKit.Infrastructure.Data.Configurations;

public class ResidentConfiguration : IEntityTypeConfiguration<Resident>
{
    public void Configure(EntityTypeBuilder<Resident> builder)
    {
        builder.HasKey(r => r.Id);
        builder.ToTable("Residents");

        builder.Property(r => r.FullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(r => r.Phone)
            .HasMaxLength(30);

        builder.Property(r => r.Email)
            .HasMaxLength(200);

        builder.Property(r => r.Role)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(r => r.IsPrimary)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(r => r.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(r => r.Notes)
            .HasMaxLength(1000);

        // Restrict delete: a unit that has residents cannot be deleted without removing residents first.
        builder.HasOne(r => r.ApartmentUnit)
            .WithMany(u => u.Residents)
            .HasForeignKey(r => r.ApartmentUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.ApartmentUnitId);
        builder.HasIndex(r => r.Role);
        builder.HasIndex(r => r.IsActive);
    }
}
