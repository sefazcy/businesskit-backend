using BusinessKit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessKit.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);

        builder.ToTable("Payments");

        builder.Property(p => p.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.Currency)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Provider)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.ProviderPaymentId)
            .HasMaxLength(500);

        builder.Property(p => p.ProviderCheckoutUrl)
            .HasMaxLength(2000);

        builder.Property(p => p.FailureReason)
            .HasMaxLength(1000);

        builder.Property(p => p.Notes)
            .HasMaxLength(1000);

        builder.HasOne(p => p.Appointment)
            .WithMany()
            .HasForeignKey(p => p.AppointmentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Customer)
            .WithMany()
            .HasForeignKey(p => p.CustomerId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(p => p.PublicToken)
            .IsRequired()
            .HasMaxLength(64);

        builder.HasIndex(p => p.PublicToken).IsUnique();

        builder.HasIndex(p => p.AppointmentId);

        // At most one Pending payment per appointment. Enforced by the database so
        // concurrent checkout requests cannot create duplicate Pending payments.
        builder.HasIndex(p => p.AppointmentId, "IX_Payments_AppointmentId_SinglePending")
            .IsUnique()
            .HasFilter("\"Status\" = 'Pending'");
        builder.HasIndex(p => p.CustomerId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.ProviderPaymentId);
    }
}
