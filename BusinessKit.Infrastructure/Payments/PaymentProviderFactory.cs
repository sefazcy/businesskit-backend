using BusinessKit.Application.Payments;
using BusinessKit.Shared.Constants;
using Microsoft.Extensions.Options;

namespace BusinessKit.Infrastructure.Payments;

public class PaymentProviderFactory : IPaymentProviderFactory
{
    private readonly ManualPaymentProvider _manual;
    private readonly PaymentProviderOptions _options;

    public PaymentProviderFactory(ManualPaymentProvider manual, IOptions<PaymentProviderOptions> options)
    {
        _manual = manual;
        _options = options.Value;
    }

    public IPaymentProvider GetProvider()
    {
        var name = string.IsNullOrWhiteSpace(_options.ActiveProvider)
            ? PaymentProviders.Manual
            : _options.ActiveProvider.Trim();

        return name switch
        {
            PaymentProviders.Manual => _manual,
            PaymentProviders.Iyzico => throw new InvalidOperationException(
                $"Payment provider '{PaymentProviders.Iyzico}' is not implemented yet."),
            _ => throw new InvalidOperationException(
                $"Payment provider '{name}' is not supported.")
        };
    }
}
