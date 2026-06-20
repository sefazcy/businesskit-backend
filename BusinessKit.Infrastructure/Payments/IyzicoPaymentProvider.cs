using BusinessKit.Application.Payments;
using BusinessKit.Application.Payments.Dtos;
using BusinessKit.Shared.Constants;
using Microsoft.Extensions.Options;

namespace BusinessKit.Infrastructure.Payments;

public class IyzicoPaymentProvider : IPaymentProvider
{
    private readonly IyzicoOptions _options;

    public IyzicoPaymentProvider(IOptions<IyzicoOptions> options)
    {
        _options = options.Value;
    }

    public string ProviderName => PaymentProviders.Iyzico;

    public Task<PaymentProviderResult> CreateCheckoutAsync(CreateCheckoutRequest request)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey) || string.IsNullOrWhiteSpace(_options.SecretKey))
        {
            return Task.FromResult(new PaymentProviderResult
            {
                IsSuccess = false,
                Provider = ProviderName,
                ErrorMessage = "Iyzico sandbox credentials are not configured.",
            });
        }

        // TODO v5.9/v6.0: call the Iyzico checkout-form initialization endpoint.
        //
        // Required request fields:
        //   conversationId  = request.PaymentId.ToString()
        //   price           = request.Amount
        //   paidPrice       = request.Amount  (no installment markup in sandbox)
        //   currency        = request.Currency  (e.g. "TRY")
        //   basketId        = $"appointment-{request.PaymentId}"
        //   callbackUrl     = _options.CallbackUrl  (v5.9 webhook endpoint)
        //   buyer.name      = request.CustomerName
        //   buyer.email     = request.CustomerEmail
        //   basketItems[0].name  = request.Description
        //   basketItems[0].price = request.Amount
        //
        // On success:
        //   IsSuccess = true
        //   Provider = ProviderName
        //   ProviderPaymentId = response.token  (Iyzico payment token)
        //   CheckoutUrl = response.paymentPageUrl
        //
        // SDK: install iyzipay NuGet package in v5.9 and replace this stub.
        return Task.FromResult(new PaymentProviderResult
        {
            IsSuccess = false,
            Provider = ProviderName,
            ErrorMessage = "Iyzico sandbox checkout initialization is not yet implemented. Complete in v5.9/v6.0.",
        });
    }

    public Task<PaymentStatusResult> GetPaymentStatusAsync(string providerPaymentId)
    {
        // TODO v5.9: retrieve payment result from Iyzico using the payment token (providerPaymentId).
        // Use IyzicoOptions.BaseUrl + ApiKey + SecretKey to authenticate the request.
        return Task.FromResult(new PaymentStatusResult
        {
            ProviderPaymentId = providerPaymentId,
            Status = PaymentStatuses.Pending,
            ErrorMessage = "Iyzico status check is not yet implemented.",
        });
    }
}
