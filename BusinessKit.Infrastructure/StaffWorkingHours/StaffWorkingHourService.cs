using BusinessKit.Application.Exceptions;
using BusinessKit.Application.StaffWorkingHours;
using BusinessKit.Application.StaffWorkingHours.Dtos;
using BusinessKit.Domain.Entities;
using BusinessKit.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BusinessKit.Infrastructure.StaffWorkingHours;

public class StaffWorkingHourService : IStaffWorkingHourService
{
    private readonly AppDbContext _context;

    public StaffWorkingHourService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<StaffWorkingHourDto> CreateAsync(CreateStaffWorkingHourDto request)
    {
        await ValidateStaffMemberExistsAsync(request.StaffMemberId);
        ValidateDayOfWeek(request.DayOfWeek);
        ValidateWorkingTimesIfRequired(request.IsWorkingDay, request.StartTime, request.EndTime);
        await EnsureNoDuplicateAsync(request.StaffMemberId, request.DayOfWeek, excludeId: null);

        var entry = new StaffWorkingHour
        {
            StaffMemberId = request.StaffMemberId,
            DayOfWeek = request.DayOfWeek,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            IsWorkingDay = request.IsWorkingDay,
            BreakStartTime = request.BreakStartTime,
            BreakEndTime = request.BreakEndTime
        };

        _context.StaffWorkingHours.Add(entry);
        await _context.SaveChangesAsync();

        await _context.Entry(entry).Reference(e => e.StaffMember).LoadAsync();

        return MapToDto(entry);
    }

    public async Task<List<StaffWorkingHourDto>> GetAllAsync(int? staffMemberId = null)
    {
        var query = _context.StaffWorkingHours
            .Include(w => w.StaffMember)
            .AsQueryable();

        if (staffMemberId.HasValue)
            query = query.Where(w => w.StaffMemberId == staffMemberId.Value);

        var entries = await query
            .OrderBy(w => w.StaffMemberId)
            .ThenBy(w => w.DayOfWeek)
            .ToListAsync();

        return entries.Select(MapToDto).ToList();
    }

    public async Task<StaffWorkingHourDto?> GetByIdAsync(int id)
    {
        var entry = await _context.StaffWorkingHours
            .Include(w => w.StaffMember)
            .FirstOrDefaultAsync(w => w.Id == id);

        return entry == null ? null : MapToDto(entry);
    }

    public async Task<List<StaffWorkingHourDto>> GetByStaffMemberIdAsync(int staffMemberId)
    {
        var exists = await _context.StaffMembers.AnyAsync(s => s.Id == staffMemberId);
        if (!exists)
            throw new InvalidStaffWorkingHourReferenceException(
                $"Staff member with ID {staffMemberId} was not found.");

        var entries = await _context.StaffWorkingHours
            .Include(w => w.StaffMember)
            .Where(w => w.StaffMemberId == staffMemberId)
            .OrderBy(w => w.DayOfWeek)
            .ToListAsync();

        return entries.Select(MapToDto).ToList();
    }

    public async Task<StaffWorkingHourDto?> UpdateAsync(int id, UpdateStaffWorkingHourDto request)
    {
        var entry = await _context.StaffWorkingHours
            .Include(w => w.StaffMember)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (entry == null)
            return null;

        ValidateDayOfWeek(request.DayOfWeek);
        ValidateWorkingTimesIfRequired(request.IsWorkingDay, request.StartTime, request.EndTime);
        await EnsureNoDuplicateAsync(entry.StaffMemberId, request.DayOfWeek, excludeId: id);

        entry.DayOfWeek = request.DayOfWeek;
        entry.StartTime = request.StartTime;
        entry.EndTime = request.EndTime;
        entry.IsWorkingDay = request.IsWorkingDay;
        entry.BreakStartTime = request.BreakStartTime;
        entry.BreakEndTime = request.BreakEndTime;

        await _context.SaveChangesAsync();

        return MapToDto(entry);
    }

    private async Task ValidateStaffMemberExistsAsync(int staffMemberId)
    {
        var exists = await _context.StaffMembers.AnyAsync(s => s.Id == staffMemberId);
        if (!exists)
            throw new InvalidStaffWorkingHourReferenceException(
                $"Staff member with ID {staffMemberId} was not found.");
    }

    private static void ValidateDayOfWeek(int dayOfWeek)
    {
        if (dayOfWeek < 1 || dayOfWeek > 7)
            throw new InvalidStaffWorkingHourException(
                $"DayOfWeek must be between 1 (Monday) and 7 (Sunday). Got: {dayOfWeek}.");
    }

    private static void ValidateWorkingTimesIfRequired(bool isWorkingDay, string? startTime, string? endTime)
    {
        if (!isWorkingDay)
            return;

        if (string.IsNullOrWhiteSpace(startTime))
            throw new InvalidStaffWorkingHourException(
                "StartTime is required when IsWorkingDay is true.");

        if (string.IsNullOrWhiteSpace(endTime))
            throw new InvalidStaffWorkingHourException(
                "EndTime is required when IsWorkingDay is true.");
    }

    private async Task EnsureNoDuplicateAsync(int staffMemberId, int dayOfWeek, int? excludeId)
    {
        var isDuplicate = await _context.StaffWorkingHours
            .AnyAsync(w =>
                w.StaffMemberId == staffMemberId &&
                w.DayOfWeek == dayOfWeek &&
                w.Id != excludeId);

        if (isDuplicate)
            throw new DuplicateStaffWorkingHourException(staffMemberId, dayOfWeek);
    }

    private static StaffWorkingHourDto MapToDto(StaffWorkingHour w) => new()
    {
        Id = w.Id,
        StaffMemberId = w.StaffMemberId,
        StaffMemberName = w.StaffMember?.FullName ?? string.Empty,
        DayOfWeek = w.DayOfWeek,
        DayName = GetDayName(w.DayOfWeek),
        StartTime = w.StartTime,
        EndTime = w.EndTime,
        IsWorkingDay = w.IsWorkingDay,
        BreakStartTime = w.BreakStartTime,
        BreakEndTime = w.BreakEndTime,
        CreatedAt = w.CreatedAt,
        UpdatedAt = w.UpdatedAt
    };

    private static string GetDayName(int dayOfWeek) => dayOfWeek switch
    {
        1 => "Monday",
        2 => "Tuesday",
        3 => "Wednesday",
        4 => "Thursday",
        5 => "Friday",
        6 => "Saturday",
        7 => "Sunday",
        _ => "Unknown"
    };
}
