using BusinessKit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessKit.Infrastructure.Data.Configurations;

public class StaffWorkingHourConfiguration : IEntityTypeConfiguration<StaffWorkingHour>
{
    public void Configure(EntityTypeBuilder<StaffWorkingHour> builder)
    {
        builder.HasKey(w => w.Id);

        builder.ToTable("StaffWorkingHours");

        builder.Property(w => w.StaffMemberId)
            .IsRequired();

        builder.Property(w => w.DayOfWeek)
            .IsRequired();

        builder.Property(w => w.StartTime)
            .HasMaxLength(20);

        builder.Property(w => w.EndTime)
            .HasMaxLength(20);

        builder.Property(w => w.IsWorkingDay)
            .HasDefaultValue(true);

        builder.Property(w => w.BreakStartTime)
            .HasMaxLength(20);

        builder.Property(w => w.BreakEndTime)
            .HasMaxLength(20);

        builder.HasOne(w => w.StaffMember)
            .WithMany()
            .HasForeignKey(w => w.StaffMemberId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(w => new { w.StaffMemberId, w.DayOfWeek })
            .IsUnique();

        builder.HasIndex(w => w.StaffMemberId);
        builder.HasIndex(w => w.DayOfWeek);
    }
}
