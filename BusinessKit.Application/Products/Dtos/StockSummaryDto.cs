namespace BusinessKit.Application.Products.Dtos;

public class StockSummaryDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal CurrentStock { get; set; }
    public decimal MinStock { get; set; }
    public bool IsLowStock { get; set; }
    public decimal TotalIn { get; set; }
    public decimal TotalOut { get; set; }
    public int AdjustmentCount { get; set; }
    public DateTime? LastMovementAt { get; set; }
}
