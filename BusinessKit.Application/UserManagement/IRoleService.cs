using BusinessKit.Application.UserManagement.Dtos;

namespace BusinessKit.Application.UserManagement;

public interface IRoleService
{
    Task<List<RoleDto>> GetAllRolesAsync();
}
