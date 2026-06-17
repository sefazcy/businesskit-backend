namespace BusinessKit.Domain.Entities;

public class StaffWorkingHour : BaseEntity
{
    public int StaffMemberId { get; set; }
    public StaffMember StaffMember { get; set; } = null!;
    public int DayOfWeek { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public bool IsWorkingDay { get; set; } = true;
    public string? BreakStartTime { get; set; }
    public string? BreakEndTime { get; set; }
}
