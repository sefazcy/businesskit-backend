namespace BusinessKit.Application.Exceptions;

public class InvalidAppointmentReferenceException : Exception
{
    public InvalidAppointmentReferenceException(string message) : base(message) { }
}
