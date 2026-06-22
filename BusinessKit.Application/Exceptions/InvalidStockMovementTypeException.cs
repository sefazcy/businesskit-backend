namespace BusinessKit.Application.Exceptions;

public class InvalidStockMovementTypeException : Exception
{
    public InvalidStockMovementTypeException(string type)
        : base($"'{type}' is not a valid stock movement type. Allowed values: In, Out, Adjustment.") { }
}
