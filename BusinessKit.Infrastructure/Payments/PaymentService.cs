using BusinessKit.Application.Common;
using BusinessKit.Domain;
using BusinessKit.Application.Email;
using BusinessKit.Application.Notifications;
using BusinessKit.Application.Payments;
using BusinessKit.Application.Payments.Dtos;
using BusinessKit.Domain.Entities;
using BusinessKit.Infrastructure.Data;
using BusinessKit.Infrastructure.Email;
using BusinessKit.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BusinessKit.Infrastructure.Payments;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _context;
    private readonly IPaymentProviderFactory _providerFactory;
    private readonly IyzicoPaymentProvider _iyzicoProvider;
    private readonly INotificationService _notificationService;
    private readonly IEmailSender _emailSender;
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<PaymentService> _logger;
    private readonly FrontendUrlOptions _frontendUrls;

    public PaymentService(
        AppDbContext context,
        IPaymentProviderFactory providerFactory,
        IyzicoPaymentProvider iyzicoProvider,
        INotificationService notificationService,
        IEmailSender emailSender,
        IOptions<EmailSettings> emailOptions,
        ILogger<PaymentService> logger,
        IOptions<FrontendUrlOptions> frontendUrlOptions)
    {
        _frontendUrls = frontendUrlOptions.Value;
        _context = context;
        _providerFactory = providerFactory;
        _iyzicoProvider = iyzicoProvider;
        _notificationService = notificationService;
        _emailSender = emailSender;
        _emailSettings = emailOptions.Value;
        _logger = logger;
    }

    public async Task<List<PaymentDto>> GetAllAsync(string? status = null, int? appointmentId = null, int take = 50)
    {
        if (take <= 0) take = 50;
        if (take > 200) take = 200;

        var query = _context.Payments.AsQueryable();

        var statusTrim = status?.Trim();
        if (!string.IsNullOrWhiteSpace(statusTrim))
            query = query.Where(p => p.Status == statusTrim);

        if (appointmentId.HasValue)
            query = query.Where(p => p.AppointmentId == appointmentId.Value);

        var payments = await query
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .Take(take)
            .ToListAsync();

        return payments.Select(MapToDto).ToList();
    }

    public async Task<PaymentDto?> GetByIdAsync(int id)
    {
        var payment = await _context.Payments.FindAsync(id);
        return payment == null ? null : MapToDto(payment);
    }

    public async Task<List<PaymentDto>> GetByAppointmentIdAsync(int appointmentId)
    {
        var payments = await _context.Payments
            .Where(p => p.AppointmentId == appointmentId)
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id)
            .ToListAsync();

        return payments.Select(MapToDto).ToList();
    }

    public async Task<PaymentDto?> CreateForAppointmentAsync(int appointmentId, CreateAdminPaymentDto dto)
    {
        var appointment = await _context.Appointments.FindAsync(appointmentId);
        if (appointment == null)
            return null;

        if (appointment.Status == AppointmentStatuses.Cancelled)
            throw new InvalidOperationException(
                "Cannot create a payment for a cancelled appointment.");

        if (await HasPendingPaymentAsync(appointmentId))
            throw new InvalidOperationException(
                "This appointment already has a Pending payment. Resolve it (mark paid or failed) before creating a new one.");

        var payment = new Payment
        {
            AppointmentId = appointmentId,
            CustomerId = appointment.CustomerId,
            Amount = dto.Amount,
            Currency = dto.Currency.Trim().ToUpperInvariant(),
            Status = PaymentStatuses.Pending,
            Provider = _providerFactory.GetProvider().ProviderName,
            Notes = dto.Notes?.Trim()
        };

        _context.Payments.Add(payment);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            // Another request created a Pending payment for this appointment concurrently.
            _context.Entry(payment).State = EntityState.Detached;
            throw new InvalidOperationException(
                "This appointment already has a Pending payment. Resolve it (mark paid or failed) before creating a new one.");
        }

        return MapToDto(payment);
    }

    public async Task<PaymentDto?> MarkPaidAsync(int id)
    {
        var payment = await _context.Payments
            .Include(p => p.Appointment)
                .ThenInclude(a => a.BusinessService)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (payment == null)
            return null;

        if (payment.Provider != PaymentProviders.Manual)
        {
            var reason = payment.Provider == PaymentProviders.Iyzico
                ? "Iyzico payments can only be marked Paid after provider callback verification."
                : $"Manual status changes are not supported for '{payment.Provider}' payments.";
            throw new InvalidOperationException(reason);
        }

        if (payment.Status != PaymentStatuses.Pending)
            throw new InvalidOperationException(
                $"Cannot mark payment as Paid from status '{payment.Status}'. Only Pending payments can be marked as Paid.");

        payment.Status = PaymentStatuses.Paid;
        payment.PaidAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        try
        {
            await _notificationService.CreateAsync(
                "Payment received",
                $"Payment of {payment.Currency} {payment.Amount:0.00} for appointment #{payment.AppointmentId} has been marked as paid.",
                NotificationTypes.PaymentReceived,
                "Payment",
                payment.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create PaymentReceived notification for payment #{Id}.", payment.Id);
        }

        if (!string.IsNullOrWhiteSpace(payment.Appointment?.CustomerEmail))
        {
            try
            {
                var appointment = payment.Appointment;
                var businessName = string.IsNullOrWhiteSpace(_emailSettings.FromName) ? "BusinessKit" : _emailSettings.FromName;

                var (subject, html) = EmailTemplates.PaymentConfirmedCustomer(
                    appointment.CustomerFullName,
                    payment.Amount,
                    payment.Currency,
                    appointment.Id,
                    appointment.RequestedDate,
                    appointment.RequestedTime,
                    appointment.BusinessService?.Title,
                    businessName);

                await _emailSender.SendAsync(appointment.CustomerEmail!, appointment.CustomerFullName, subject, html);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send payment confirmation email for payment #{Id}.", payment.Id);
            }
        }

        return MapToDto(payment);
    }

    public async Task<PaymentDto?> MarkFailedAsync(int id, MarkPaymentFailedDto dto)
    {
        var payment = await _context.Payments.FindAsync(id);
        if (payment == null)
            return null;

        if (payment.Provider != PaymentProviders.Manual)
        {
            var reason = payment.Provider == PaymentProviders.Iyzico
                ? "Iyzico payments cannot be manually marked as Failed."
                : $"Manual status changes are not supported for '{payment.Provider}' payments.";
            throw new InvalidOperationException(reason);
        }

        if (payment.Status != PaymentStatuses.Pending)
            throw new InvalidOperationException(
                $"Cannot mark payment as Failed from status '{payment.Status}'. Only Pending payments can be marked as Failed.");

        payment.Status = PaymentStatuses.Failed;
        payment.FailedAt = DateTime.UtcNow;
        payment.FailureReason = dto.FailureReason?.Trim();

        await _context.SaveChangesAsync();

        try
        {
            await _notificationService.CreateAsync(
                "Payment failed",
                $"Payment of {payment.Currency} {payment.Amount:0.00} for appointment #{payment.AppointmentId} has been marked as failed.",
                NotificationTypes.PaymentFailed,
                "Payment",
                payment.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create PaymentFailed notification for payment #{Id}.", payment.Id);
        }

        return MapToDto(payment);
    }

    public async Task<PaymentDto?> MarkRefundedAsync(int id, MarkPaymentRefundedDto dto)
    {
        var payment = await _context.Payments.FindAsync(id);
        if (payment == null)
            return null;

        if (payment.Provider != PaymentProviders.Manual)
        {
            var reason = payment.Provider == PaymentProviders.Iyzico
                ? "Iyzico refund is not implemented yet. Process refunds through the Iyzico merchant dashboard."
                : $"Manual status changes are not supported for '{payment.Provider}' payments.";
            throw new InvalidOperationException(reason);
        }

        if (payment.Status != PaymentStatuses.Paid)
            throw new InvalidOperationException(
                $"Cannot mark payment as Refunded from status '{payment.Status}'. Only Paid payments can be marked as Refunded.");

        payment.Status = PaymentStatuses.Refunded;
        payment.RefundedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(dto.Notes))
            payment.Notes = dto.Notes.Trim();

        await _context.SaveChangesAsync();

        return MapToDto(payment);
    }

    public async Task<int?> FindIdByPublicTokenAsync(string paymentToken)
    {
        if (!PublicTokens.IsWellFormed(paymentToken))
            return null;

        return await _context.Payments
            .Where(p => p.PublicToken == paymentToken)
            .Select(p => (int?)p.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<PublicPaymentStatusDto?> GetPublicStatusAsync(string paymentToken)
    {
        if (!PublicTokens.IsWellFormed(paymentToken))
            return null;

        var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PublicToken == paymentToken);
        if (payment == null)
            return null;

        return new PublicPaymentStatusDto
        {
            Id = payment.Id,
            Status = payment.Status,
            Amount = payment.Amount,
            Currency = payment.Currency,
            Provider = payment.Provider,
            CheckoutUrl = payment.ProviderCheckoutUrl,
            PaidAt = payment.PaidAt,
            FailedAt = payment.FailedAt,
            RefundedAt = payment.RefundedAt,
        };
    }

    public async Task<PaymentCheckoutResponseDto?> CreatePublicCheckoutAsync(string appointmentToken)
    {
        if (!PublicTokens.IsWellFormed(appointmentToken))
            return null;

        var appointment = await _context.Appointments
            .Include(a => a.BusinessService)
            .FirstOrDefaultAsync(a => a.PublicToken == appointmentToken);

        if (appointment == null)
            return null;

        var appointmentId = appointment.Id;

        if (appointment.Status == AppointmentStatuses.Cancelled)
            throw new InvalidOperationException(
                "This appointment has been cancelled. Payment cannot be started.");

        if (appointment.BusinessServiceId == null || appointment.BusinessService == null)
            throw new InvalidOperationException(
                "Appointment does not have an associated service. Cannot determine payment amount.");

        var amount = appointment.BusinessService.Price;
        if (amount <= 0)
            throw new InvalidOperationException(
                "The service for this appointment has no price configured. Cannot create a payment.");

        // Idempotency: return the existing Pending payment if one already exists
        var existingPending = await _context.Payments
            .FirstOrDefaultAsync(p => p.AppointmentId == appointmentId && p.Status == PaymentStatuses.Pending);

        if (existingPending != null)
            return MapToCheckoutDto(existingPending, "Pending payment already exists for this appointment.");

        // Block checkout if appointment already has a completed payment
        var hasPaid = await _context.Payments
            .AnyAsync(p => p.AppointmentId == appointmentId && p.Status == PaymentStatuses.Paid);

        if (hasPaid)
            throw new InvalidOperationException(
                "Appointment already has a completed payment. No new checkout session can be created.");

        // Resolve currency from business settings; fall back to TRY for missing or invalid stored values
        var settings = await _context.BusinessSettings.FirstOrDefaultAsync();
        var rawCurrency = settings?.Currency?.Trim().ToUpperInvariant();
        var currency = CurrencyCodes.IsValid(rawCurrency) ? rawCurrency! : CurrencyCodes.TRY;

        var provider = _providerFactory.GetProvider();

        var payment = new Payment
        {
            AppointmentId = appointmentId,
            CustomerId = appointment.CustomerId,
            Amount = amount,
            Currency = currency,
            Status = PaymentStatuses.Pending,
            Provider = provider.ProviderName,
        };

        _context.Payments.Add(payment);
        try
        {
            await _context.SaveChangesAsync(); // persists so payment.Id is assigned
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            // A concurrent request created the Pending payment first (unique index on
            // AppointmentId where Status = 'Pending'). Return that one — idempotent.
            _context.Entry(payment).State = EntityState.Detached;
            var concurrentPending = await _context.Payments
                .FirstOrDefaultAsync(p => p.AppointmentId == appointmentId && p.Status == PaymentStatuses.Pending);
            if (concurrentPending == null)
                throw;
            return MapToCheckoutDto(concurrentPending, "Pending payment already exists for this appointment.");
        }

        var checkoutRequest = new CreateCheckoutRequest
        {
            PaymentId = payment.Id,
            Amount = payment.Amount,
            Currency = payment.Currency,
            CustomerName = appointment.CustomerFullName,
            CustomerEmail = appointment.CustomerEmail,
            Description = appointment.BusinessService?.Title ?? $"Appointment #{appointmentId}",
            ReturnUrl = _frontendUrls.BuildPaymentStatusUrl(payment.PublicToken),
            CancelUrl = _frontendUrls.BuildPaymentStatusUrl(payment.PublicToken),
        };

        var providerResult = await provider.CreateCheckoutAsync(checkoutRequest);

        if (!providerResult.IsSuccess)
        {
            // Mark as Failed so this record is not returned by the Pending idempotency check on retry.
            payment.Status = PaymentStatuses.Failed;
            payment.FailedAt = DateTime.UtcNow;
            payment.FailureReason = providerResult.ErrorMessage;
            await _context.SaveChangesAsync();

            // Provider/config details stay in logs and FailureReason (admin-visible);
            // the public caller only gets a generic message.
            _logger.LogError(
                "Checkout initialization failed for payment #{PaymentId} (appointment #{AppointmentId}, provider {Provider}): {Error}",
                payment.Id, appointmentId, provider.ProviderName, providerResult.ErrorMessage);

            throw new InvalidOperationException(
                "Online payment could not be started right now. Please try again later or contact us.");
        }

        payment.ProviderPaymentId = providerResult.ProviderPaymentId;
        payment.ProviderCheckoutUrl = providerResult.CheckoutUrl;
        await _context.SaveChangesAsync();

        return MapToCheckoutDto(payment, "Checkout session created. Awaiting payment.");
    }

    public async Task<PaymentStatsDto> GetStatsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Payments.AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(p => p.CreatedAt >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(p => p.CreatedAt <= toDate.Value);

        var rows = await query
            .Select(p => new { p.Currency, p.Status, p.Amount })
            .ToListAsync();

        var totalsByCurrency = rows
            .GroupBy(p => string.IsNullOrWhiteSpace(p.Currency)
                ? "UNKNOWN"
                : p.Currency.Trim().ToUpperInvariant())
            .OrderBy(g => g.Key)
            .Select(g => new PaymentCurrencySummaryDto
            {
                Currency = g.Key,
                PendingAmount  = g.Where(p => p.Status == PaymentStatuses.Pending).Sum(p => p.Amount),
                PaidAmount     = g.Where(p => p.Status == PaymentStatuses.Paid).Sum(p => p.Amount),
                FailedAmount   = g.Where(p => p.Status == PaymentStatuses.Failed).Sum(p => p.Amount),
                RefundedAmount = g.Where(p => p.Status == PaymentStatuses.Refunded).Sum(p => p.Amount),
                TotalAmount    = g.Sum(p => p.Amount),
            })
            .ToList();

        return new PaymentStatsDto
        {
            TotalCount    = rows.Count,
            PendingCount  = rows.Count(p => p.Status == PaymentStatuses.Pending),
            PaidCount     = rows.Count(p => p.Status == PaymentStatuses.Paid),
            FailedCount   = rows.Count(p => p.Status == PaymentStatuses.Failed),
            RefundedCount = rows.Count(p => p.Status == PaymentStatuses.Refunded),
            TotalsByCurrency = totalsByCurrency,
        };
    }

    public async Task<PaymentCallbackResult> HandleIyzicoCallbackAsync(IyzicoCallbackRequest request)
    {
        var token = request.Token?.Trim();
        if (string.IsNullOrWhiteSpace(token))
        {
            return new PaymentCallbackResult
            {
                IsVerified = false,
                Message = "Token is required.",
            };
        }

        // Look up by provider token — never trust client-supplied paymentId for security decisions
        var payment = await _context.Payments
            .Include(p => p.Appointment)
                .ThenInclude(a => a.BusinessService)
            .FirstOrDefaultAsync(p => p.ProviderPaymentId == token);

        if (payment == null)
        {
            _logger.LogWarning("Iyzico callback received for unknown token (length {Len}).", token.Length);
            return new PaymentCallbackResult
            {
                IsVerified = false,
                Message = "No payment found for the provided token.",
            };
        }

        if (payment.Provider != PaymentProviders.Iyzico)
        {
            return new PaymentCallbackResult
            {
                IsVerified = false,
                Message = "Payment was not created with the Iyzico provider.",
                PaymentId = payment.Id,
                PaymentToken = payment.PublicToken,
                Status = payment.Status,
            };
        }

        // Already in a terminal state — return without re-processing
        if (payment.Status == PaymentStatuses.Paid)
        {
            return new PaymentCallbackResult
            {
                IsVerified = true,
                Message = "Payment was already verified and marked as Paid.",
                PaymentId = payment.Id,
                PaymentToken = payment.PublicToken,
                Status = payment.Status,
            };
        }

        if (payment.Status is PaymentStatuses.Failed or PaymentStatuses.Refunded or PaymentStatuses.Cancelled)
        {
            return new PaymentCallbackResult
            {
                IsVerified = false,
                Message = $"Payment is already in terminal status '{payment.Status}'.",
                PaymentId = payment.Id,
                PaymentToken = payment.PublicToken,
                Status = payment.Status,
            };
        }

        // Verify with Iyzico — only provider-side confirmation can mark a payment Paid
        var verification = await _iyzicoProvider.GetPaymentStatusAsync(token);

        if (verification.Status == PaymentStatuses.Paid)
        {
            // The provider says "paid" — make sure it is *this* payment, for the right amount.
            var mismatch = GetVerificationMismatch(payment, verification);
            if (mismatch != null)
            {
                payment.FailureReason = $"Verification mismatch: {mismatch}";
                await _context.SaveChangesAsync();

                _logger.LogError(
                    "Iyzico callback: payment #{Id} NOT marked Paid — verification mismatch: {Mismatch}. Manual review required.",
                    payment.Id, mismatch);

                return new PaymentCallbackResult
                {
                    IsVerified = false,
                    Message = "Payment could not be verified. It has been flagged for manual review.",
                    PaymentId = payment.Id,
                    PaymentToken = payment.PublicToken,
                    Status = payment.Status,
                };
            }

            payment.Status = PaymentStatuses.Paid;
            payment.PaidAt ??= DateTime.UtcNow;
            payment.FailureReason = null;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Iyzico callback: payment #{Id} verified and marked Paid.", payment.Id);

            try
            {
                await _notificationService.CreateAsync(
                    "Payment received",
                    $"Payment of {payment.Currency} {payment.Amount:0.00} for appointment #{payment.AppointmentId} was verified via Iyzico.",
                    NotificationTypes.PaymentReceived,
                    "Payment",
                    payment.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create PaymentReceived notification for payment #{Id}.", payment.Id);
            }

            if (!string.IsNullOrWhiteSpace(payment.Appointment?.CustomerEmail))
            {
                try
                {
                    var appointment = payment.Appointment;
                    var businessName = string.IsNullOrWhiteSpace(_emailSettings.FromName) ? "BusinessKit" : _emailSettings.FromName;

                    var (subject, html) = EmailTemplates.PaymentConfirmedCustomer(
                        appointment.CustomerFullName,
                        payment.Amount,
                        payment.Currency,
                        appointment.Id,
                        appointment.RequestedDate,
                        appointment.RequestedTime,
                        appointment.BusinessService?.Title,
                        businessName);

                    await _emailSender.SendAsync(appointment.CustomerEmail, appointment.CustomerFullName, subject, html);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send payment confirmation email for payment #{Id}.", payment.Id);
                }
            }

            return new PaymentCallbackResult
            {
                IsVerified = true,
                Message = "Payment verified and marked as Paid.",
                PaymentId = payment.Id,
                PaymentToken = payment.PublicToken,
                Status = payment.Status,
            };
        }

        if (verification.Status == PaymentStatuses.Failed)
        {
            payment.Status = PaymentStatuses.Failed;
            payment.FailedAt ??= DateTime.UtcNow;
            payment.FailureReason = verification.ErrorMessage;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Iyzico callback: payment #{Id} marked Failed. Reason: {Reason}.",
                payment.Id, verification.ErrorMessage);

            try
            {
                await _notificationService.CreateAsync(
                    "Payment failed",
                    $"Payment of {payment.Currency} {payment.Amount:0.00} for appointment #{payment.AppointmentId} failed via Iyzico.",
                    NotificationTypes.PaymentFailed,
                    "Payment",
                    payment.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to create PaymentFailed notification for payment #{Id}.", payment.Id);
            }

            return new PaymentCallbackResult
            {
                IsVerified = false,
                Message = "Payment was declined by the provider.",
                PaymentId = payment.Id,
                PaymentToken = payment.PublicToken,
                Status = payment.Status,
            };
        }

        // Pending / indeterminate — leave status unchanged
        return new PaymentCallbackResult
        {
            IsVerified = false,
            Message = verification.ErrorMessage ?? "Payment verification did not confirm completion. Status unchanged.",
            PaymentId = payment.Id,
            PaymentToken = payment.PublicToken,
            Status = payment.Status,
        };
    }

    private Task<bool> HasPendingPaymentAsync(int appointmentId) =>
        _context.Payments.AnyAsync(p => p.AppointmentId == appointmentId && p.Status == PaymentStatuses.Pending);

    /// <summary>
    /// Compares what the provider verified against the local payment record.
    /// Returns a description of the first mismatch, or null when everything matches.
    /// </summary>
    private static string? GetVerificationMismatch(Payment payment, PaymentStatusResult verification)
    {
        var expectedBasketId = IyzicoPaymentProvider.BuildBasketId(payment.Id);
        if (!string.Equals(verification.MerchantReference, expectedBasketId, StringComparison.Ordinal))
            return $"basketId '{verification.MerchantReference}' does not match expected '{expectedBasketId}'";

        if (verification.Amount == null)
            return "provider did not report a parseable amount";

        if (decimal.Round(verification.Amount.Value, 2) != decimal.Round(payment.Amount, 2))
            return $"amount {verification.Amount.Value:0.00} does not match expected {payment.Amount:0.00}";

        if (!string.Equals(verification.Currency?.Trim(), payment.Currency, StringComparison.OrdinalIgnoreCase))
            return $"currency '{verification.Currency}' does not match expected '{payment.Currency}'";

        return null;
    }

    /// <summary>SQLite constraint violation (error code 19), e.g. the unique Pending-payment index.</summary>
    private static bool IsUniqueConstraintViolation(DbUpdateException ex) =>
        ex.InnerException is Microsoft.Data.Sqlite.SqliteException { SqliteErrorCode: 19 };

    private static PaymentDto MapToDto(Payment p) => new()
    {
        Id = p.Id,
        AppointmentId = p.AppointmentId,
        CustomerId = p.CustomerId,
        Amount = p.Amount,
        Currency = p.Currency,
        Status = p.Status,
        Provider = p.Provider,
        ProviderPaymentId = p.ProviderPaymentId,
        ProviderCheckoutUrl = p.ProviderCheckoutUrl,
        PaidAt = p.PaidAt,
        FailedAt = p.FailedAt,
        RefundedAt = p.RefundedAt,
        FailureReason = p.FailureReason,
        Notes = p.Notes,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };

    private static PaymentSummaryDto MapToSummary(Payment p) => new()
    {
        Id = p.Id,
        Amount = p.Amount,
        Currency = p.Currency,
        Status = p.Status,
        Provider = p.Provider,
        PaidAt = p.PaidAt,
        FailedAt = p.FailedAt,
        RefundedAt = p.RefundedAt,
        CreatedAt = p.CreatedAt
    };

    private static PaymentCheckoutResponseDto MapToCheckoutDto(Payment p, string message) => new()
    {
        PaymentToken = p.PublicToken,
        PaymentId = p.Id,
        AppointmentId = p.AppointmentId,
        Amount = p.Amount,
        Currency = p.Currency,
        Status = p.Status,
        Provider = p.Provider,
        CheckoutUrl = p.ProviderCheckoutUrl,
        Message = message,
    };
}
