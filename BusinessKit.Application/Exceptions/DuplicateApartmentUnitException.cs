namespace BusinessKit.Application.Exceptions;

public class DuplicateApartmentUnitException : Exception
{
    public DuplicateApartmentUnitException(string? blockName, string doorNumber)
        : base(blockName == null
            ? $"An apartment unit with door number '{doorNumber}' already exists."
            : $"An apartment unit with block '{blockName}' and door number '{doorNumber}' already exists.") { }
}
