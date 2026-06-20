namespace BusinessKit.Application.Payments.Dtos;

public class PaymentStatsDto
{
    public int TotalCount { get; set; }
    public int PendingCount { get; set; }
    public int PaidCount { get; set; }
    public int FailedCount { get; set; }
    public int RefundedCount { get; set; }
    public List<PaymentCurrencySummaryDto> TotalsByCurrency { get; set; } = [];
}
