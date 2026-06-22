namespace BusinessKit.Application.Products.Dtos;

public class ProductListQuery
{
    public string? Search { get; set; }
    public string? Category { get; set; }
    public bool? IsActive { get; set; }
    public bool LowStockOnly { get; set; } = false;
    public int Take { get; set; } = 50;
}
