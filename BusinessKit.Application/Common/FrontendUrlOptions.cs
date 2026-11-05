namespace BusinessKit.Application.Common;

/// <summary>
/// Public URLs of the frontend applications. Used for CORS origins and for
/// building browser-facing links (e.g. payment return / status pages).
/// Override per environment via environment variables, e.g.
/// <c>FrontendUrls__PublicSiteUrl=https://www.example.com</c>.
/// </summary>
public class FrontendUrlOptions
{
    public const string SectionName = "FrontendUrls";

    public string PublicSiteUrl { get; set; } = "http://localhost:5174";

    public string AdminPanelUrl { get; set; } = "http://localhost:5173";

    /// <summary>Builds the public payment status page URL from a payment's public token.</summary>
    public string BuildPaymentStatusUrl(string? paymentToken) =>
        $"{PublicSiteUrl.TrimEnd('/')}/payment-status/{(string.IsNullOrWhiteSpace(paymentToken) ? "unknown" : Uri.EscapeDataString(paymentToken))}";
}
