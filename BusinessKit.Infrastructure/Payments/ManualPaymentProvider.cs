using BusinessKit.Application.Payments;
using BusinessKit.Application.Payments.Dtos;
using BusinessKit.Shared.Constants;

namespace BusinessKit.Infrastructure.Payments;

public class ManualPaymentProvider : IPaymentProvider
{
    public string ProviderName => PaymentProviders.Manual;

    public Task<PaymentProviderResult> CreateCheckoutAsync(CreateCheckoutRequest request)
        => Task.FromResult(new PaymentProviderResult
        {
            IsSuccess = true,
            Provider = ProviderName,
            CheckoutUrl = $"http://localhost:5174/payment-status/{request.PaymentId}",
        });

    public Task<PaymentStatusResult> GetPaymentStatusAsync(string providerPaymentId)
        => Task.FromResult(new PaymentStatusResult
        {
            ProviderPaymentId = providerPaymentId,
            Status = PaymentStatuses.Pending
        });
}
