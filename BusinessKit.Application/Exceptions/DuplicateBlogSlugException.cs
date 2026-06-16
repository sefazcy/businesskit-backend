namespace BusinessKit.Application.Exceptions;

public class DuplicateBlogSlugException : Exception
{
    public DuplicateBlogSlugException(string slug, string language)
        : base($"A blog post with slug '{slug}' already exists for language '{language}'.") { }
}
