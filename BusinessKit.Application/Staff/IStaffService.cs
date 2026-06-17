using BusinessKit.Application.Staff.Dtos;

namespace BusinessKit.Application.Staff;

public interface IStaffService
{
    Task<List<StaffMemberDto>> GetActiveStaffAsync();
    Task<StaffMemberDto?> GetActiveStaffBySlugAsync(string slug);

    Task<List<StaffMemberDto>> GetAllStaffAsync(bool? isActive);
    Task<StaffMemberDto?> GetStaffByIdAsync(int id);
    Task<StaffMemberDto> CreateStaffAsync(CreateStaffMemberDto dto);
    Task<StaffMemberDto?> UpdateStaffAsync(int id, UpdateStaffMemberDto dto);
    Task<StaffMemberDto?> ToggleActiveAsync(int id);
}
