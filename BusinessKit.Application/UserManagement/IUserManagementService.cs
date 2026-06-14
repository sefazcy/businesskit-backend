using BusinessKit.Application.UserManagement.Dtos;

namespace BusinessKit.Application.UserManagement;

public interface IUserManagementService
{
    Task<List<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<UserDto> CreateUserAsync(CreateUserDto dto);
    Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto dto);
    Task<UserDto?> ToggleActiveAsync(int id);
}
