using System.ComponentModel.DataAnnotations;

namespace BusinessKit.Application.Payments.Dtos;

public class MarkPaymentFailedDto
{
    [MaxLength(1000)]
    public string? FailureReason { get; set; }
}
