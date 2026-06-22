namespace BusinessKit.Application.Products.Dtos;

public class StockMovementListQuery
{
    public int? ProductId { get; set; }
    public string? Type { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int Take { get; set; } = 100;
}
