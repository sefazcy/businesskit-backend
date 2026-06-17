namespace BusinessKit.Application.StaffWorkingHours.Dtos;

public class StaffWorkingHourDto
{
    public int Id { get; set; }
    public int StaffMemberId { get; set; }
    public string StaffMemberName { get; set; } = string.Empty;
    public int DayOfWeek { get; set; }
    public string DayName { get; set; } = string.Empty;
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public bool IsWorkingDay { get; set; }
    public string? BreakStartTime { get; set; }
    public string? BreakEndTime { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
