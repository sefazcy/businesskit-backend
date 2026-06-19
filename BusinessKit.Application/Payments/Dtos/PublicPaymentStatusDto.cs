namespace BusinessKit.Application.Payments.Dtos;

public class PublicPaymentStatusDto
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }
}
