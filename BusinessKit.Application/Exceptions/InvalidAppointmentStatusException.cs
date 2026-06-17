namespace BusinessKit.Application.Exceptions;

public class InvalidAppointmentStatusException : Exception
{
    public InvalidAppointmentStatusException(string status)
        : base($"'{status}' is not a valid appointment status. Allowed values: Pending, Confirmed, Cancelled, Completed.") { }
}
