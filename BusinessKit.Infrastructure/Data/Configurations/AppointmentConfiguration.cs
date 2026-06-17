using BusinessKit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessKit.Infrastructure.Data.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.HasKey(a => a.Id);

        builder.ToTable("Appointments");

        builder.Property(a => a.CustomerFullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(a => a.CustomerEmail)
            .HasMaxLength(200);

        builder.Property(a => a.CustomerPhone)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(a => a.RequestedDate)
            .IsRequired();

        builder.Property(a => a.RequestedTime)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(a => a.Note)
            .HasMaxLength(1000);

        builder.Property(a => a.Status)
            .IsRequired()
            .HasMaxLength(30)
            .HasDefaultValue("Pending");

        builder.Property(a => a.AdminNote)
            .HasMaxLength(1000);

        // Optional FK to StaffMembers — SetNull so existing appointments are preserved if a staff member is deleted
        builder.HasOne(a => a.StaffMember)
            .WithMany()
            .HasForeignKey(a => a.StaffMemberId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // Optional FK to BusinessServices — same rationale
        builder.HasOne(a => a.BusinessService)
            .WithMany()
            .HasForeignKey(a => a.BusinessServiceId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.RequestedDate);
        builder.HasIndex(a => a.StaffMemberId);
        builder.HasIndex(a => a.BusinessServiceId);
    }
}
