namespace BusinessKit.Application.Payments.Dtos;

public class PaymentCheckoutResponseDto
{
    /// <summary>Use this for the public status page / API. PaymentId is for display only.</summary>
    public string PaymentToken { get; set; } = string.Empty;
    public int PaymentId { get; set; }
    public int AppointmentId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string? CheckoutUrl { get; set; }
    public string Message { get; set; } = string.Empty;
}
