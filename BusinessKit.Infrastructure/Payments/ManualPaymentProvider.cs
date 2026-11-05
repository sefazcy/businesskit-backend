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
            // No external payment page for manual payments — point to the public status page
            // (built by PaymentService from FrontendUrls + the payment's public token).
            CheckoutUrl = string.IsNullOrWhiteSpace(request.ReturnUrl) ? null : request.ReturnUrl,
        });

    public Task<PaymentStatusResult> GetPaymentStatusAsync(string providerPaymentId)
        => Task.FromResult(new PaymentStatusResult
        {
            ProviderPaymentId = providerPaymentId,
            Status = PaymentStatuses.Pending
        });
}
