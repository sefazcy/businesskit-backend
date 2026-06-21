namespace BusinessKit.Application.Payments.Dtos;

public class PaymentCallbackResult
{
    public bool IsVerified { get; set; }
    public string Message { get; set; } = string.Empty;
}
