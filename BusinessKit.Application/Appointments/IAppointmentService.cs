using BusinessKit.Application.Appointments.Dtos;

namespace BusinessKit.Application.Appointments;

public interface IAppointmentService
{
    Task<AppointmentDto> CreateAsync(CreateAppointmentRequestDto dto);
    Task<List<AppointmentDto>> GetAllAsync(string? status, int? staffMemberId, int? businessServiceId, DateTime? date, DateTime? startDate = null, DateTime? endDate = null);
    Task<AppointmentDto?> GetByIdAsync(int id);
    Task<AppointmentDto?> UpdateStatusAsync(int id, UpdateAppointmentStatusDto dto);
    Task<AppointmentDto?> UpdateAsync(int id, UpdateAppointmentDto dto);
    Task<List<AppointmentDto>> GetTodayAsync(string? status, int? staffMemberId, int? businessServiceId);
    Task<List<AppointmentDto>> GetUpcomingAsync(string? status, int? staffMemberId, int? businessServiceId, int days);
    Task<AppointmentStatsDto> GetStatsAsync(int? staffMemberId, int? businessServiceId, DateTime? startDate, DateTime? endDate);
}
