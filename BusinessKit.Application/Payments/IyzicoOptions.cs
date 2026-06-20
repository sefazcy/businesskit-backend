namespace BusinessKit.Application.Payments;

public class IyzicoOptions
{
    public const string SectionName = "Iyzico";

    public string BaseUrl { get; set; } = "https://sandbox-api.iyzipay.com";
    public string ApiKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    // CallbackUrl is where Iyzico POSTs the payment result (v5.9 webhook endpoint)
    public string CallbackUrl { get; set; } = string.Empty;
}
