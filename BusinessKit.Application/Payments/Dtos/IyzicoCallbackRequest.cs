namespace BusinessKit.Application.Payments.Dtos;

public class IyzicoCallbackRequest
{
    public string? Token { get; set; }
    public int? PaymentId { get; set; }
}
