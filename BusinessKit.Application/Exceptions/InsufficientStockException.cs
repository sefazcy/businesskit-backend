namespace BusinessKit.Application.Exceptions;

public class InsufficientStockException : Exception
{
    public InsufficientStockException(string productName, decimal currentStock, decimal requested)
        : base($"Insufficient stock for '{productName}'. Current stock is {currentStock}, but {requested} was requested for Out movement.") { }
}
