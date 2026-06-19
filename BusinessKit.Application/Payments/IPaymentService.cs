using BusinessKit.Application.Payments.Dtos;

namespace BusinessKit.Application.Payments;

public interface IPaymentService
{
    Task<List<PaymentDto>> GetAllAsync(string? status = null, int? appointmentId = null, int take = 50);
    Task<PaymentDto?> GetByIdAsync(int id);
    Task<List<PaymentDto>> GetByAppointmentIdAsync(int appointmentId);
    Task<PaymentDto?> CreateForAppointmentAsync(int appointmentId, CreateAdminPaymentDto dto);
    Task<PaymentDto?> MarkPaidAsync(int id);
    Task<PaymentDto?> MarkFailedAsync(int id, MarkPaymentFailedDto dto);
    Task<PaymentDto?> MarkRefundedAsync(int id, MarkPaymentRefundedDto dto);
    Task<PublicPaymentStatusDto?> GetPublicStatusAsync(int id);
}
