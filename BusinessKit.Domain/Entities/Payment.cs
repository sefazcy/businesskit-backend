namespace BusinessKit.Domain.Entities;

public class Payment : BaseEntity
{
    /// <summary>Unguessable id used by public endpoints and links (never the numeric Id).</summary>
    public string PublicToken { get; set; } = PublicTokens.New();

    public int AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;

    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }

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
}
