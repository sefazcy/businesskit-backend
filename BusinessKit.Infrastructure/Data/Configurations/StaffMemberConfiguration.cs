using BusinessKit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessKit.Infrastructure.Data.Configurations;

public class StaffMemberConfiguration : IEntityTypeConfiguration<StaffMember>
{
    public void Configure(EntityTypeBuilder<StaffMember> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.FullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(s => s.Slug)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(s => s.Slug)
            .IsUnique();

        builder.Property(s => s.Title)
            .HasMaxLength(150);

        builder.Property(s => s.Bio)
            .HasMaxLength(2000);

        builder.Property(s => s.PhotoUrl)
            .HasMaxLength(500);

        builder.Property(s => s.Email)
            .HasMaxLength(200);

        builder.Property(s => s.Phone)
            .HasMaxLength(30);

        builder.Property(s => s.InstagramUrl)
            .HasMaxLength(500);

        builder.Property(s => s.LinkedInUrl)
            .HasMaxLength(500);

        builder.Property(s => s.IsActive)
            .HasDefaultValue(true);

        builder.Property(s => s.DisplayOrder)
            .HasDefaultValue(0);
    }
}
