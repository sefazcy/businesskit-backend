using System.Globalization;
using BusinessKit.Application.Payments;
using BusinessKit.Application.Payments.Dtos;
using BusinessKit.Shared.Constants;
using Iyzipay.Model;
using Iyzipay.Request;
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

    public async Task<PaymentProviderResult> CreateCheckoutAsync(CreateCheckoutRequest request)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey) || string.IsNullOrWhiteSpace(_options.SecretKey))
        {
            return new PaymentProviderResult
            {
                IsSuccess = false,
                Provider = ProviderName,
                ErrorMessage = "Iyzico sandbox credentials are not configured.",
            };
        }

        if (string.IsNullOrWhiteSpace(_options.CallbackUrl))
        {
            return new PaymentProviderResult
            {
                IsSuccess = false,
                Provider = ProviderName,
                ErrorMessage = "Iyzico CallbackUrl is not configured. Set Iyzico:CallbackUrl in user secrets or appsettings.Local.json.",
            };
        }

        var iyziOptions = new Iyzipay.Options
        {
            ApiKey = _options.ApiKey,
            SecretKey = _options.SecretKey,
            BaseUrl = _options.BaseUrl,
        };

        // Decimal amounts must use period separator, exactly two decimal places
        var amountStr = request.Amount.ToString("0.00", CultureInfo.InvariantCulture);

        // Split name into first/last; sandbox placeholders for required Iyzico fields not in our domain
        var nameParts = (request.CustomerName ?? string.Empty)
            .Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var firstName = nameParts.Length > 0 ? nameParts[0] : "Customer";
        var lastName  = nameParts.Length > 1 ? nameParts[1] : "Customer";
        var email = !string.IsNullOrWhiteSpace(request.CustomerEmail)
            ? request.CustomerEmail
            : "customer@businesskit.local";

        var buyer = new Buyer
        {
            Id                  = $"customer-{request.PaymentId}",
            Name                = firstName,
            Surname             = lastName,
            Email               = email,
            GsmNumber           = "+905350000000",    // sandbox placeholder
            IdentityNumber      = "74300864791",      // sandbox test TCKN
            RegistrationDate    = "2023-01-01 00:00:00",
            LastLoginDate       = "2023-01-01 00:00:00",
            RegistrationAddress = "BusinessKit",
            Ip                  = "127.0.0.1",
            City                = "Istanbul",
            Country             = "Turkey",
            ZipCode             = "34000",
        };

        var shippingAddress = new Address
        {
            ContactName = request.CustomerName,
            Description = request.Description,
            City        = "Istanbul",
            Country     = "Turkey",
            ZipCode     = "34000",
        };

        var billingAddress = new Address
        {
            ContactName = request.CustomerName,
            Description = request.Description,
            City        = "Istanbul",
            Country     = "Turkey",
            ZipCode     = "34000",
        };

        var basketItems = new List<BasketItem>
        {
            new BasketItem
            {
                Id        = $"item-{request.PaymentId}",
                Name      = request.Description,
                Category1 = "Services",
                ItemType  = BasketItemType.VIRTUAL.ToString(),
                Price     = amountStr,
            },
        };

        var checkoutRequest = new CreateCheckoutFormInitializeRequest
        {
            Locale              = Locale.TR.ToString(),
            ConversationId      = request.PaymentId.ToString(),
            Price               = amountStr,
            PaidPrice           = amountStr,
            Currency            = request.Currency,
            BasketId            = $"appointment-{request.PaymentId}",
            PaymentGroup        = PaymentGroup.PRODUCT.ToString(),
            CallbackUrl         = _options.CallbackUrl,
            EnabledInstallments = new List<int> { 1 },
            Buyer               = buyer,
            ShippingAddress     = shippingAddress,
            BillingAddress      = billingAddress,
            BasketItems         = basketItems,
        };

        CheckoutFormInitialize response;
        try
        {
            response = await CheckoutFormInitialize.Create(checkoutRequest, iyziOptions);
        }
        catch (Exception ex)
        {
            return new PaymentProviderResult
            {
                IsSuccess = false,
                Provider = ProviderName,
                ErrorMessage = $"Iyzico sandbox request failed: {ex.Message}",
            };
        }

        if (response.Status != Status.SUCCESS.ToString())
        {
            return new PaymentProviderResult
            {
                IsSuccess = false,
                Provider = ProviderName,
                ErrorMessage = $"Iyzico checkout initialization failed: {response.ErrorMessage} (code: {response.ErrorCode})",
            };
        }

        return new PaymentProviderResult
        {
            IsSuccess = true,
            Provider = ProviderName,
            ProviderPaymentId = response.Token,
            CheckoutUrl = response.PaymentPageUrl,
        };
    }

    public Task<PaymentStatusResult> GetPaymentStatusAsync(string providerPaymentId)
    {
        // TODO v6.1: call Iyzico retrieve-payment endpoint with providerPaymentId (the checkout token).
        // Use IyzicoOptions.BaseUrl + ApiKey + SecretKey to sign the request.
        // On success: map Iyzico payment status to PaymentStatuses constants.
        return Task.FromResult(new PaymentStatusResult
        {
            ProviderPaymentId = providerPaymentId,
            Status = PaymentStatuses.Pending,
            ErrorMessage = "Iyzico payment status verification is not yet implemented. Complete in v6.1.",
        });
    }
}
