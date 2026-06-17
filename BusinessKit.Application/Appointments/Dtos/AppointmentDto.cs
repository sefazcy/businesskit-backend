namespace BusinessKit.Application.Appointments.Dtos;

public class AppointmentDto
{
    public int Id { get; set; }
    public string CustomerFullName { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public string CustomerPhone { get; set; } = string.Empty;
    public int? StaffMemberId { get; set; }
    public string? StaffMemberName { get; set; }
    public int? BusinessServiceId { get; set; }
    public string? BusinessServiceTitle { get; set; }
    public DateTime RequestedDate { get; set; }
    public string RequestedTime { get; set; } = string.Empty;
    public string? Note { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? AdminNote { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
