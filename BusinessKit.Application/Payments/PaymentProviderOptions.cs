using BusinessKit.Shared.Constants;

namespace BusinessKit.Application.Payments;

public class PaymentProviderOptions
{
    public const string SectionName = "PaymentProvider";

    public string ActiveProvider { get; set; } = PaymentProviders.Manual;
}
