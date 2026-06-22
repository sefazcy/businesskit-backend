namespace BusinessKit.Application.Payments.Dtos;

public class PaymentCallbackResult
{
    public bool IsVerified { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? PaymentId { get; set; }
    public string? Status { get; set; }
}
