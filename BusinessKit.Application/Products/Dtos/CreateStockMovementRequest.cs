using System.ComponentModel.DataAnnotations;

namespace BusinessKit.Application.Products.Dtos;

public class CreateStockMovementRequest
{
    [Required(ErrorMessage = "ProductId is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "ProductId must be a positive integer.")]
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Type is required.")]
    public string Type { get; set; } = string.Empty;

    // For In and Out: must be > 0.
    // For Adjustment: must be >= 0 (represents the final target stock value).
    [Range(0, double.MaxValue, ErrorMessage = "Quantity cannot be negative.")]
    public decimal Quantity { get; set; }

    [MaxLength(150)]
    public string? Reason { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}
