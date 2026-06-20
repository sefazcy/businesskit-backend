namespace BusinessKit.Application.Payments;

public interface IPaymentProviderFactory
{
    /// <summary>
    /// Returns the active payment provider.
    /// v5.7: will accept settings/config to select between Manual, Iyzico, etc.
    /// </summary>
    IPaymentProvider GetProvider();
}
