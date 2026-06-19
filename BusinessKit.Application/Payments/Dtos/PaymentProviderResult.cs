namespace BusinessKit.Application.Payments.Dtos;

public class PaymentProviderResult
{
    public bool Success { get; set; }
    public string? ProviderPaymentId { get; set; }
    public string? CheckoutUrl { get; set; }
    public string? ErrorMessage { get; set; }
}
