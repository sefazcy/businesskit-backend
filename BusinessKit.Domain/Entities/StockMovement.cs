namespace BusinessKit.Domain.Entities;

public class StockMovement : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    // "In" | "Out" | "Adjustment" — see StockMovementTypes constants.
    // In:         newStock = previousStock + quantity
    // Out:        newStock = previousStock - quantity  (errors if result < 0)
    // Adjustment: newStock = quantity  (quantity is the final target stock value)
    public string Type { get; set; } = string.Empty;

    public decimal Quantity { get; set; }
    public decimal PreviousStock { get; set; }
    public decimal NewStock { get; set; }

    public string? Reason { get; set; }
    public string? Notes { get; set; }
}
