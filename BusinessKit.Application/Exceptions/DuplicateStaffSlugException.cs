namespace BusinessKit.Application.Exceptions;

public class DuplicateStaffSlugException : Exception
{
    public DuplicateStaffSlugException(string slug)
        : base($"A staff member with slug '{slug}' already exists.") { }
}
