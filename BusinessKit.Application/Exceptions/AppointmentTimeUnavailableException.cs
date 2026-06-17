namespace BusinessKit.Application.Exceptions;

public class AppointmentTimeUnavailableException : Exception
{
    public AppointmentTimeUnavailableException(string time)
        : base($"The requested time '{time}' is already booked for this staff member on that date.") { }
}
