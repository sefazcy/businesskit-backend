namespace BusinessKit.Application.Payments;

public class IyzicoOptions
{
    public const string SectionName = "Iyzico";

    public string BaseUrl { get; set; } = "https://sandbox-api.iyzipay.com";
    public string ApiKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string CallbackUrl { get; set; } = string.Empty;

    // Set at startup via PostConfigure — never read from config file.
    // True when ASPNETCORE_ENVIRONMENT is Development. Allows localhost callbacks.
    public bool IsDevelopment { get; set; }
}
