using BusinessKit.Application.ApartmentManagement;
using BusinessKit.Application.ApartmentManagement.Dtos;
using BusinessKit.Application.Exceptions;
using BusinessKit.Domain.Entities;
using BusinessKit.Infrastructure.Data;
using BusinessKit.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace BusinessKit.Infrastructure.ApartmentManagement;

public class ApartmentUnitService : IApartmentUnitService
{
    private readonly AppDbContext _context;

    public ApartmentUnitService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ApartmentUnitDto>> GetAllAsync(ApartmentUnitListQuery query)
    {
        var q = _context.ApartmentUnits.AsQueryable();

        if (query.IsActive.HasValue)
            q = q.Where(u => u.IsActive == query.IsActive.Value);

        if (query.IsOccupied.HasValue)
            q = q.Where(u => u.IsOccupied == query.IsOccupied.Value);

        var searchTerm = query.Search?.Trim();
        if (!string.IsNullOrWhiteSpace(searchTerm))
            q = q.Where(u =>
                EF.Functions.Like(u.DoorNumber, $"%{searchTerm}%") ||
                EF.Functions.Like(u.BlockName ?? "", $"%{searchTerm}%") ||
                EF.Functions.Like(u.UnitType, $"%{searchTerm}%") ||
                EF.Functions.Like(u.Notes ?? "", $"%{searchTerm}%"));

        var blockTerm = query.BlockName?.Trim();
        if (!string.IsNullOrWhiteSpace(blockTerm))
            q = q.Where(u => EF.Functions.Like(u.BlockName ?? "", $"%{blockTerm}%"));

        var unitTypeTerm = query.UnitType?.Trim();
        if (!string.IsNullOrWhiteSpace(unitTypeTerm))
            q = q.Where(u => u.UnitType == unitTypeTerm);

        return await q
            .OrderBy(u => u.BlockName)
            .ThenBy(u => u.DoorNumber)
            .ThenBy(u => u.Id)
            .Take(query.Take > 0 ? query.Take : 100)
            .Select(u => new ApartmentUnitDto
            {
                Id                  = u.Id,
                BlockName           = u.BlockName,
                FloorNumber         = u.FloorNumber,
                DoorNumber          = u.DoorNumber,
                UnitType            = u.UnitType,
                GrossArea           = u.GrossArea,
                NetArea             = u.NetArea,
                IsOccupied          = u.IsOccupied,
                IsActive            = u.IsActive,
                Notes               = u.Notes,
                CreatedAt           = u.CreatedAt,
                UpdatedAt           = u.UpdatedAt,
                ResidentCount       = u.Residents.Count(r => r.IsActive),
                PrimaryResidentName = u.Residents
                    .Where(r => r.IsPrimary && r.IsActive)
                    .Select(r => r.FullName)
                    .FirstOrDefault()
            })
            .ToListAsync();
    }

    public async Task<ApartmentUnitDto?> GetByIdAsync(int id)
    {
        return await _context.ApartmentUnits
            .Where(u => u.Id == id)
            .Select(u => new ApartmentUnitDto
            {
                Id                  = u.Id,
                BlockName           = u.BlockName,
                FloorNumber         = u.FloorNumber,
                DoorNumber          = u.DoorNumber,
                UnitType            = u.UnitType,
                GrossArea           = u.GrossArea,
                NetArea             = u.NetArea,
                IsOccupied          = u.IsOccupied,
                IsActive            = u.IsActive,
                Notes               = u.Notes,
                CreatedAt           = u.CreatedAt,
                UpdatedAt           = u.UpdatedAt,
                ResidentCount       = u.Residents.Count(r => r.IsActive),
                PrimaryResidentName = u.Residents
                    .Where(r => r.IsPrimary && r.IsActive)
                    .Select(r => r.FullName)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ApartmentUnitDto> CreateAsync(CreateApartmentUnitRequest request)
    {
        if (!ApartmentUnitTypes.IsValid(request.UnitType))
            throw new InvalidUnitTypeException(request.UnitType);

        var blockName  = string.IsNullOrWhiteSpace(request.BlockName) ? null : request.BlockName.Trim();
        var doorNumber = request.DoorNumber.Trim();

        await EnsureUnitUniqueAsync(blockName, doorNumber, excludeId: null);

        var unit = new ApartmentUnit
        {
            BlockName   = blockName,
            FloorNumber = request.FloorNumber,
            DoorNumber  = doorNumber,
            UnitType    = request.UnitType.Trim(),
            GrossArea   = request.GrossArea,
            NetArea     = request.NetArea,
            IsOccupied  = request.IsOccupied,
            IsActive    = request.IsActive,
            Notes       = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
        };

        _context.ApartmentUnits.Add(unit);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(unit.Id))!;
    }

    public async Task<ApartmentUnitDto?> UpdateAsync(int id, UpdateApartmentUnitRequest request)
    {
        var unit = await _context.ApartmentUnits.FindAsync(id);
        if (unit == null)
            return null;

        if (!ApartmentUnitTypes.IsValid(request.UnitType))
            throw new InvalidUnitTypeException(request.UnitType);

        var blockName  = string.IsNullOrWhiteSpace(request.BlockName) ? null : request.BlockName.Trim();
        var doorNumber = request.DoorNumber.Trim();

        await EnsureUnitUniqueAsync(blockName, doorNumber, excludeId: id);

        unit.BlockName   = blockName;
        unit.FloorNumber = request.FloorNumber;
        unit.DoorNumber  = doorNumber;
        unit.UnitType    = request.UnitType.Trim();
        unit.GrossArea   = request.GrossArea;
        unit.NetArea     = request.NetArea;
        unit.IsOccupied  = request.IsOccupied;
        unit.IsActive    = request.IsActive;
        unit.Notes       = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<ApartmentUnitDto?> ToggleActiveAsync(int id)
    {
        var unit = await _context.ApartmentUnits.FindAsync(id);
        if (unit == null)
            return null;

        unit.IsActive = !unit.IsActive;
        await _context.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    private async Task EnsureUnitUniqueAsync(string? blockName, string doorNumber, int? excludeId)
    {
        var taken = await _context.ApartmentUnits.AnyAsync(u =>
            u.BlockName == blockName &&
            u.DoorNumber == doorNumber &&
            (excludeId == null || u.Id != excludeId));

        if (taken)
            throw new DuplicateApartmentUnitException(blockName, doorNumber);
    }
}
