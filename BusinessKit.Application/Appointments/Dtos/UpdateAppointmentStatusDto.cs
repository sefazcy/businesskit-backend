using System.ComponentModel.DataAnnotations;

namespace BusinessKit.Application.Appointments.Dtos;

public class UpdateAppointmentStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? AdminNote { get; set; }
}
