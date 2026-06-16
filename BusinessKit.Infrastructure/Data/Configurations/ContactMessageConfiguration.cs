using BusinessKit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessKit.Infrastructure.Data.Configurations;

public class ContactMessageConfiguration : IEntityTypeConfiguration<ContactMessage>
{
    public void Configure(EntityTypeBuilder<ContactMessage> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.FullName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.Phone)
            .HasMaxLength(30);

        builder.Property(m => m.Subject)
            .HasMaxLength(200);

        builder.Property(m => m.Message)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(m => m.IsRead)
            .HasDefaultValue(false);

        builder.Property(m => m.IsReplied)
            .HasDefaultValue(false);

        builder.Property(m => m.IsArchived)
            .HasDefaultValue(false);

        builder.Property(m => m.IpAddress)
            .HasMaxLength(45);
    }
}
