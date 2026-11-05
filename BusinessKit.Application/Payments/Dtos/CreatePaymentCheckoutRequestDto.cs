using System.ComponentModel.DataAnnotations;

namespace BusinessKit.Application.Payments.Dtos;

public class CreatePaymentCheckoutRequestDto
{
    /// <summary>The appointment's public token, returned when the appointment was created.</summary>
    [Required(ErrorMessage = "A valid appointmentToken is required.")]
    [MaxLength(64)]
    public string AppointmentToken { get; set; } = string.Empty;
}
