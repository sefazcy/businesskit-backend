namespace BusinessKit.Application.Payments.Dtos;

public class PaymentStatusResult
{
    public string ProviderPaymentId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public DateTime? PaidAt { get; set; }

    /// <summary>Amount the provider reports for this payment (null if not reported or unparseable).</summary>
    public decimal? Amount { get; set; }

    /// <summary>Currency the provider reports for this payment.</summary>
    public string? Currency { get; set; }

    /// <summary>Our own reference echoed back by the provider (Iyzico: basketId).</summary>
    public string? MerchantReference { get; set; }
}
