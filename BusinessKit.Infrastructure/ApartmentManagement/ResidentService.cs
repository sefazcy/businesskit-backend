using BusinessKit.Application.ApartmentManagement;
using BusinessKit.Application.ApartmentManagement.Dtos;
using BusinessKit.Application.Exceptions;
using BusinessKit.Domain.Entities;
using BusinessKit.Infrastructure.Data;
using BusinessKit.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace BusinessKit.Infrastructure.ApartmentManagement;

public class ResidentService : IResidentService
{
    private readonly AppDbContext _context;

    public ResidentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ResidentDto>> GetAllAsync(ResidentListQuery query)
    {
        var q = _context.Residents.Include(r => r.ApartmentUnit).AsQueryable();

        if (query.ApartmentUnitId.HasValue)
            q = q.Where(r => r.ApartmentUnitId == query.ApartmentUnitId.Value);

        if (!string.IsNullOrWhiteSpace(query.Role))
            q = q.Where(r => r.Role == query.Role.Trim());

        if (query.IsActive.HasValue)
            q = q.Where(r => r.IsActive == query.IsActive.Value);

        var searchTerm = query.Search?.Trim();
        if (!string.IsNullOrWhiteSpace(searchTerm))
            q = q.Where(r =>
                EF.Functions.Like(r.FullName, $"%{searchTerm}%") ||
                EF.Functions.Like(r.Email ?? "", $"%{searchTerm}%") ||
                EF.Functions.Like(r.Phone ?? "", $"%{searchTerm}%"));

        var residents = await q
            .OrderBy(r => r.FullName)
            .ThenBy(r => r.Id)
            .Take(query.Take > 0 ? query.Take : 100)
            .ToListAsync();

        return residents.Select(MapToDto).ToList();
    }

    public async Task<ResidentDto?> GetByIdAsync(int id)
    {
        var resident = await _context.Residents
            .Include(r => r.ApartmentUnit)
            .FirstOrDefaultAsync(r => r.Id == id);

        return resident == null ? null : MapToDto(resident);
    }

    public async Task<List<ResidentDto>> GetByUnitIdAsync(int apartmentUnitId)
    {
        var residents = await _context.Residents
            .Include(r => r.ApartmentUnit)
            .Where(r => r.ApartmentUnitId == apartmentUnitId)
            .OrderByDescending(r => r.IsActive)
            .ThenBy(r => r.FullName)
            .Take(500)
            .ToListAsync();

        return residents.Select(MapToDto).ToList();
    }

    public async Task<ResidentDto> CreateAsync(CreateResidentRequest request)
    {
        if (!ResidentRoles.IsValid(request.Role))
            throw new InvalidResidentRoleException(request.Role);

        var unit = await _context.ApartmentUnits.FindAsync(request.ApartmentUnitId);
        if (unit == null)
            throw new KeyNotFoundException($"Apartment unit with id {request.ApartmentUnitId} was not found.");

        if (request.MoveInDate.HasValue && request.MoveOutDate.HasValue
            && request.MoveOutDate.Value < request.MoveInDate.Value)
            throw new ArgumentException("MoveOutDate cannot be earlier than MoveInDate.");

        var resident = new Resident
        {
            ApartmentUnitId = request.ApartmentUnitId,
            FullName        = request.FullName.Trim(),
            Phone           = string.IsNullOrWhiteSpace(request.Phone)  ? null : request.Phone.Trim(),
            Email           = string.IsNullOrWhiteSpace(request.Email)  ? null : request.Email.Trim(),
            Role            = request.Role.Trim(),
            IsPrimary       = request.IsPrimary,
            IsActive        = request.IsActive,
            MoveInDate      = request.MoveInDate,
            MoveOutDate     = request.MoveOutDate,
            Notes           = string.IsNullOrWhiteSpace(request.Notes)  ? null : request.Notes.Trim(),
        };

        _context.Residents.Add(resident);
        await _context.SaveChangesAsync();

        resident.ApartmentUnit = unit;
        return MapToDto(resident);
    }

    public async Task<ResidentDto?> UpdateAsync(int id, UpdateResidentRequest request)
    {
        if (!ResidentRoles.IsValid(request.Role))
            throw new InvalidResidentRoleException(request.Role);

        var resident = await _context.Residents
            .Include(r => r.ApartmentUnit)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (resident == null)
            return null;

        if (request.MoveInDate.HasValue && request.MoveOutDate.HasValue
            && request.MoveOutDate.Value < request.MoveInDate.Value)
            throw new ArgumentException("MoveOutDate cannot be earlier than MoveInDate.");

        resident.FullName    = request.FullName.Trim();
        resident.Phone       = string.IsNullOrWhiteSpace(request.Phone)  ? null : request.Phone.Trim();
        resident.Email       = string.IsNullOrWhiteSpace(request.Email)  ? null : request.Email.Trim();
        resident.Role        = request.Role.Trim();
        resident.IsPrimary   = request.IsPrimary;
        resident.IsActive    = request.IsActive;
        resident.MoveInDate  = request.MoveInDate;
        resident.MoveOutDate = request.MoveOutDate;
        resident.Notes       = string.IsNullOrWhiteSpace(request.Notes)  ? null : request.Notes.Trim();

        await _context.SaveChangesAsync();
        return MapToDto(resident);
    }

    public async Task<ResidentDto?> ToggleActiveAsync(int id)
    {
        var resident = await _context.Residents
            .Include(r => r.ApartmentUnit)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (resident == null)
            return null;

        resident.IsActive = !resident.IsActive;
        await _context.SaveChangesAsync();
        return MapToDto(resident);
    }

    private static ResidentDto MapToDto(Resident r) => new()
    {
        Id                  = r.Id,
        ApartmentUnitId     = r.ApartmentUnitId,
        ApartmentDoorNumber = r.ApartmentUnit.DoorNumber,
        ApartmentBlockName  = r.ApartmentUnit.BlockName,
        FullName            = r.FullName,
        Phone               = r.Phone,
        Email               = r.Email,
        Role                = r.Role,
        IsPrimary           = r.IsPrimary,
        IsActive            = r.IsActive,
        MoveInDate          = r.MoveInDate,
        MoveOutDate         = r.MoveOutDate,
        Notes               = r.Notes,
        CreatedAt           = r.CreatedAt,
        UpdatedAt           = r.UpdatedAt,
    };
}
