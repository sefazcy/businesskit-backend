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
    private readonly IPaymentProvider _paymentProvider;
    private readonly INotificationService _notificationService;
    private readonly IEmailSender _emailSender;
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        AppDbContext context,
        IPaymentProvider paymentProvider,
        INotificationService notificationService,
        IEmailSender emailSender,
        IOptions<EmailSettings> emailOptions,
        ILogger<PaymentService> logger)
    {
        _context = context;
        _paymentProvider = paymentProvider;
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

        var payment = new Payment
        {
            AppointmentId = appointmentId,
            CustomerId = appointment.CustomerId,
            Amount = dto.Amount,
            Currency = dto.Currency.Trim().ToUpperInvariant(),
            Status = PaymentStatuses.Pending,
            Provider = _paymentProvider.ProviderName,
            Notes = dto.Notes?.Trim()
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

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

    public async Task<PublicPaymentStatusDto?> GetPublicStatusAsync(int id)
    {
        var payment = await _context.Payments.FindAsync(id);
        if (payment == null)
            return null;

        return new PublicPaymentStatusDto
        {
            Id = payment.Id,
            Status = payment.Status,
            PaidAt = payment.PaidAt
        };
    }

    public async Task<PaymentCheckoutResponseDto?> CreatePublicCheckoutAsync(int appointmentId)
    {
        var appointment = await _context.Appointments
            .Include(a => a.BusinessService)
            .FirstOrDefaultAsync(a => a.Id == appointmentId);

        if (appointment == null)
            return null;

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

        var payment = new Payment
        {
            AppointmentId = appointmentId,
            CustomerId = appointment.CustomerId,
            Amount = amount,
            Currency = currency,
            Status = PaymentStatuses.Pending,
            Provider = _paymentProvider.ProviderName,
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(); // persists so payment.Id is assigned

        // Build placeholder checkout URL with the newly assigned payment ID
        payment.ProviderCheckoutUrl = $"http://localhost:5174/payment-status/{payment.Id}";
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
