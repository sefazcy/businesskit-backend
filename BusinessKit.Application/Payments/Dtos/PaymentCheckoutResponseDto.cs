namespace BusinessKit.Application.Payments.Dtos;

public class PaymentCheckoutResponseDto
{
    public int PaymentId { get; set; }
    public int AppointmentId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string? CheckoutUrl { get; set; }
    public string Message { get; set; } = string.Empty;
}
