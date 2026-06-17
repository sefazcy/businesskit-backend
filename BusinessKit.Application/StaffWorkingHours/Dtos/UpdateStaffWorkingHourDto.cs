using System.ComponentModel.DataAnnotations;

namespace BusinessKit.Application.StaffWorkingHours.Dtos;

public class UpdateStaffWorkingHourDto
{
    [Required]
    [Range(1, 7, ErrorMessage = "DayOfWeek must be between 1 (Monday) and 7 (Sunday).")]
    public int DayOfWeek { get; set; }

    [MaxLength(20)]
    public string? StartTime { get; set; }

    [MaxLength(20)]
    public string? EndTime { get; set; }

    public bool IsWorkingDay { get; set; } = true;

    [MaxLength(20)]
    public string? BreakStartTime { get; set; }

    [MaxLength(20)]
    public string? BreakEndTime { get; set; }
}
