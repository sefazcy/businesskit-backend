namespace BusinessKit.Application.Exceptions;

public class InvalidAppointmentTimeException : Exception
{
    public InvalidAppointmentTimeException(string message) : base(message) { }
}
