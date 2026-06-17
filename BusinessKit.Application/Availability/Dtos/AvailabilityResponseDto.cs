namespace BusinessKit.Application.Availability.Dtos;

public class AvailabilityResponseDto
{
    public int StaffMemberId { get; set; }
    public string StaffMemberName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int DayOfWeek { get; set; }
    public string DayName { get; set; } = string.Empty;
    public int SlotDurationMinutes { get; set; }
    public List<AvailableSlotDto> Slots { get; set; } = new();
}
