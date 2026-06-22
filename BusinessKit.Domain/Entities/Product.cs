namespace BusinessKit.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public string? Category { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal CurrentStock { get; set; } = 0;
    public decimal MinStock { get; set; } = 0;
    public decimal CostPrice { get; set; } = 0;
    public decimal SalePrice { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
}
