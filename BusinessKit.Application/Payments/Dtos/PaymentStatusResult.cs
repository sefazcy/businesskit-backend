namespace BusinessKit.Application.Payments.Dtos;

public class PaymentStatusResult
{
    public string ProviderPaymentId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public DateTime? PaidAt { get; set; }
}
