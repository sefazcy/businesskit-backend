using System.ComponentModel.DataAnnotations;

namespace BusinessKit.Application.Payments.Dtos;

public class CreateAdminPaymentDto
{
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Notes { get; set; }
}
