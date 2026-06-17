namespace BusinessKit.Application.Appointments.Dtos;

public class AppointmentStatsDto
{
    public int TotalAppointments { get; set; }
    public int PendingCount { get; set; }
    public int ConfirmedCount { get; set; }
    public int CancelledCount { get; set; }
    public int CompletedCount { get; set; }
    public int TodayCount { get; set; }
    public int Upcoming7DaysCount { get; set; }
}
