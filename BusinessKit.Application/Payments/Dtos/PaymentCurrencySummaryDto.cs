namespace BusinessKit.Application.Payments.Dtos;

public class PaymentCurrencySummaryDto
{
    public string Currency { get; set; } = string.Empty;
    public decimal PendingAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal FailedAmount { get; set; }
    public decimal RefundedAmount { get; set; }
    public decimal TotalAmount { get; set; }
}
