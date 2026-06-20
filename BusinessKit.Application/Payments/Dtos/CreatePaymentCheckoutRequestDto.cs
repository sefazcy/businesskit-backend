using System.ComponentModel.DataAnnotations;

namespace BusinessKit.Application.Payments.Dtos;

public class CreatePaymentCheckoutRequestDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "A valid appointmentId is required.")]
    public int AppointmentId { get; set; }
}
