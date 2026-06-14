using BusinessKit.Application.Auth;
using BusinessKit.Application.Exceptions;
using BusinessKit.Application.UserManagement;
using BusinessKit.Application.UserManagement.Dtos;
using BusinessKit.Domain.Entities;
using BusinessKit.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BusinessKit.Infrastructure.UserManagement;

public class UserManagementService : IUserManagementService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public UserManagementService(AppDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .OrderBy(u => u.Id)
            .ToListAsync();

        return users.Select(MapToDto).ToList();
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id);

        return user == null ? null : MapToDto(user);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
    {
        var roles = await ValidateRolesAsync(dto.Roles);

        var emailTaken = await _context.Users.AnyAsync(u => u.Email == dto.Email);
        if (emailTaken)
            throw new DuplicateEmailException(dto.Email);

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = _passwordHasher.Hash(dto.Password),
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        foreach (var role in roles)
        {
            _context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        }
        await _context.SaveChangesAsync();

        return (await GetUserByIdAsync(user.Id))!;
    }

    public async Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto dto)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return null;

        var roles = await ValidateRolesAsync(dto.Roles);

        var emailTaken = await _context.Users.AnyAsync(u => u.Email == dto.Email && u.Id != id);
        if (emailTaken)
            throw new DuplicateEmailException(dto.Email);

        user.FullName = dto.FullName;
        user.Email = dto.Email;
        user.IsActive = dto.IsActive;

        _context.UserRoles.RemoveRange(user.UserRoles);
        foreach (var role in roles)
        {
            _context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        }

        await _context.SaveChangesAsync();

        return (await GetUserByIdAsync(user.Id))!;
    }

    public async Task<UserDto?> ToggleActiveAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return null;

        user.IsActive = !user.IsActive;
        await _context.SaveChangesAsync();

        return (await GetUserByIdAsync(user.Id))!;
    }

    private async Task<List<Role>> ValidateRolesAsync(List<string> roleNames)
    {
        if (roleNames == null || roleNames.Count == 0)
            throw new InvalidRoleException("At least one role must be specified.");

        var roles = await _context.Roles
            .Where(r => roleNames.Contains(r.Name))
            .ToListAsync();

        var invalidRoles = roleNames.Except(roles.Select(r => r.Name)).ToList();
        if (invalidRoles.Count > 0)
            throw new InvalidRoleException($"Invalid role(s): {string.Join(", ", invalidRoles)}.");

        return roles;
    }

    private static UserDto MapToDto(User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email,
        IsActive = user.IsActive,
        Roles = user.UserRoles.Select(ur => ur.Role.Name).OrderBy(n => n).ToList(),
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt
    };
}
