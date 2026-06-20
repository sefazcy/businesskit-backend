using BusinessKit.Application.Payments;

namespace BusinessKit.Infrastructure.Payments;

public class PaymentProviderFactory : IPaymentProviderFactory
{
    private readonly ManualPaymentProvider _manual;

    public PaymentProviderFactory(ManualPaymentProvider manual)
    {
        _manual = manual;
    }

    // v5.7: inject IOptions<PaymentSettings> and switch on configured provider name
    public IPaymentProvider GetProvider() => _manual;
}
