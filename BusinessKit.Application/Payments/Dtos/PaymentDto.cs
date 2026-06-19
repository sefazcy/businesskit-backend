namespace BusinessKit.Application.Payments.Dtos;

public class PaymentDto
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public int? CustomerId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string? ProviderPaymentId { get; set; }
    public string? ProviderCheckoutUrl { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public DateTime? RefundedAt { get; set; }
    public string? FailureReason { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
