using BusinessKit.Application.Payments.Dtos;

namespace BusinessKit.Application.Payments;

public interface IPaymentProvider
{
    string ProviderName { get; }
    Task<PaymentProviderResult> CreateCheckoutAsync(CreateCheckoutRequest request);
    Task<PaymentStatusResult> GetPaymentStatusAsync(string providerPaymentId);
}
