using System.ComponentModel.DataAnnotations;

namespace BusinessKit.Application.Appointments.Dtos;

public class UpdateAppointmentDto
{
    [Required]
    [MaxLength(150)]
    public string CustomerFullName { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(200)]
    public string? CustomerEmail { get; set; }

    [Required]
    [MaxLength(30)]
    public string CustomerPhone { get; set; } = string.Empty;

    public int? StaffMemberId { get; set; }

    public int? BusinessServiceId { get; set; }

    [Required]
    public DateTime RequestedDate { get; set; }

    [Required]
    [MaxLength(20)]
    public string RequestedTime { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Note { get; set; }

    [Required]
    public string Status { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? AdminNote { get; set; }

    public int? CustomerId { get; set; }
}
