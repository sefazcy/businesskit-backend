using BusinessKit.Application.Appointments.Dtos;

namespace BusinessKit.Application.Appointments;

public interface IAppointmentService
{
    Task<AppointmentDto> CreateAsync(CreateAppointmentRequestDto dto);
    Task<List<AppointmentDto>> GetAllAsync(string? status, int? staffMemberId, int? businessServiceId, DateTime? date);
    Task<AppointmentDto?> GetByIdAsync(int id);
    Task<AppointmentDto?> UpdateStatusAsync(int id, UpdateAppointmentStatusDto dto);
    Task<AppointmentDto?> UpdateAsync(int id, UpdateAppointmentDto dto);
}
