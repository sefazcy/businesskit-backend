using System.ComponentModel.DataAnnotations;

namespace BusinessKit.Application.Payments.Dtos;

public class MarkPaymentRefundedDto
{
    [MaxLength(1000)]
    public string? Notes { get; set; }
}
