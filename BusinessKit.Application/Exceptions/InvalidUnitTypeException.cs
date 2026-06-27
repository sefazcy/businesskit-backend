namespace BusinessKit.Application.Exceptions;

public class InvalidUnitTypeException : Exception
{
    public InvalidUnitTypeException(string type)
        : base($"'{type}' is not a valid unit type. Allowed values: Apartment, Office, Shop, Other.") { }
}
