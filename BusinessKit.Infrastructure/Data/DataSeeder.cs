using BusinessKit.Application.Auth;
using BusinessKit.Domain.Entities;
using BusinessKit.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace BusinessKit.Infrastructure.Data;

// Development-only seeder. Seeds default roles and admin user if they do not exist.
// The default admin credentials are for local development ONLY.
// Remove or gate this behind environment checks before deploying to production.
public class DataSeeder
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public DataSeeder(AppDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task SeedAsync()
    {
        await SeedRolesAsync();
        await SeedAdminUserAsync();
    }

    private async Task SeedRolesAsync()
    {
        var roleNames = new[] { Roles.Admin, Roles.Manager, Roles.Viewer };
        foreach (var roleName in roleNames)
        {
            if (!await _context.Roles.AnyAsync(r => r.Name == roleName))
                _context.Roles.Add(new Role { Name = roleName });
        }
        await _context.SaveChangesAsync();
    }

    private async Task SeedAdminUserAsync()
    {
        const string adminEmail = "admin@businesskit.local";

        if (await _context.Users.AnyAsync(u => u.Email == adminEmail))
            return;

        var adminUser = new User
        {
            FullName = "System Admin",
            Email = adminEmail,
            PasswordHash = _passwordHasher.Hash("Admin123!"),
            IsActive = true
        };

        _context.Users.Add(adminUser);
        await _context.SaveChangesAsync();

        var adminRole = await _context.Roles.FirstAsync(r => r.Name == Roles.Admin);
        _context.UserRoles.Add(new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id });
        await _context.SaveChangesAsync();
    }
}
