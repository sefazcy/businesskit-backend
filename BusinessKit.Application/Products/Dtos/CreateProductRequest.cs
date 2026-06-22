using System.ComponentModel.DataAnnotations;

namespace BusinessKit.Application.Products.Dtos;

public class CreateProductRequest
{
    [Required(ErrorMessage = "Name is required.")]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(80)]
    public string? Sku { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    [Required(ErrorMessage = "Unit is required.")]
    [MaxLength(30)]
    public string Unit { get; set; } = string.Empty;

    [Range(0, double.MaxValue, ErrorMessage = "CurrentStock cannot be negative.")]
    public decimal CurrentStock { get; set; } = 0;

    [Range(0, double.MaxValue, ErrorMessage = "MinStock cannot be negative.")]
    public decimal MinStock { get; set; } = 0;

    [Range(0, double.MaxValue, ErrorMessage = "CostPrice cannot be negative.")]
    public decimal CostPrice { get; set; } = 0;

    [Range(0, double.MaxValue, ErrorMessage = "SalePrice cannot be negative.")]
    public decimal SalePrice { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    [MaxLength(1000)]
    public string? Notes { get; set; }
}
