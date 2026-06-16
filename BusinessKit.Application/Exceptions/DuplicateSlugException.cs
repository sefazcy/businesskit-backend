namespace BusinessKit.Application.Exceptions;

public class DuplicateSlugException : Exception
{
    public DuplicateSlugException(string slug)
        : base($"A service with slug '{slug}' already exists.") { }
}
