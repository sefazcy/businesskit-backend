namespace BusinessKit.Application.Exceptions;

public class InvalidStaffWorkingHourException : Exception
{
    public InvalidStaffWorkingHourException(string message) : base(message) { }
}
