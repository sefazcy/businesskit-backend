using BusinessKit.Application.StaffWorkingHours.Dtos;

namespace BusinessKit.Application.StaffWorkingHours;

public interface IStaffWorkingHourService
{
    Task<StaffWorkingHourDto> CreateAsync(CreateStaffWorkingHourDto request);
    Task<List<StaffWorkingHourDto>> GetAllAsync(int? staffMemberId = null);
    Task<StaffWorkingHourDto?> GetByIdAsync(int id);
    Task<List<StaffWorkingHourDto>> GetByStaffMemberIdAsync(int staffMemberId);
    Task<StaffWorkingHourDto?> UpdateAsync(int id, UpdateStaffWorkingHourDto request);
}
