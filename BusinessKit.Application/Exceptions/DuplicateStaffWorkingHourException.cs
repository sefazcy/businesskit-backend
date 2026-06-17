namespace BusinessKit.Application.Exceptions;

public class DuplicateStaffWorkingHourException : Exception
{
    public DuplicateStaffWorkingHourException(int staffMemberId, int dayOfWeek)
        : base($"A working hour entry for staff member {staffMemberId} on day {dayOfWeek} already exists.") { }
}
