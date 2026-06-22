namespace BusinessKit.Application.Products.Dtos;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public string? Category { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal CurrentStock { get; set; }
    public decimal MinStock { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SalePrice { get; set; }
    public bool IsActive { get; set; }
    public bool IsLowStock { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
