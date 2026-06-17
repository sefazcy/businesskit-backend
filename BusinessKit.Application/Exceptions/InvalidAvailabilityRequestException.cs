namespace BusinessKit.Application.Exceptions;

public class InvalidAvailabilityRequestException : Exception
{
    public InvalidAvailabilityRequestException(string message) : base(message) { }
}
