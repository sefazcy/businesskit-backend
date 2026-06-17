using BusinessKit.Application.Availability;
using BusinessKit.Application.Availability.Dtos;
using BusinessKit.Application.Exceptions;
using BusinessKit.Infrastructure.Data;
using BusinessKit.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace BusinessKit.Infrastructure.Availability;

public class AvailabilityService : IAvailabilityService
{
    private const int DefaultSlotDurationMinutes = 30;

    private readonly AppDbContext _context;

    public AvailabilityService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AvailabilityResponseDto> GetAvailableSlotsAsync(
        int staffMemberId,
        DateTime date,
        int? businessServiceId = null)
    {
        var staffMember = await _context.StaffMembers.FindAsync(staffMemberId);
        if (staffMember == null)
            throw new InvalidAvailabilityReferenceException(
                $"Staff member with ID {staffMemberId} was not found.");

        int slotDuration = DefaultSlotDurationMinutes;

        if (businessServiceId.HasValue)
        {
            var service = await _context.BusinessServices.FindAsync(businessServiceId.Value);
            if (service == null)
                throw new InvalidAvailabilityReferenceException(
                    $"Business service with ID {businessServiceId.Value} was not found.");

            slotDuration = service.DurationMinutes > 0 ? service.DurationMinutes : DefaultSlotDurationMinutes;
        }

        var projectDayOfWeek = ToProjectDayOfWeek(date.DayOfWeek);
        var dayName = GetDayName(projectDayOfWeek);

        var baseResponse = new AvailabilityResponseDto
        {
            StaffMemberId = staffMemberId,
            StaffMemberName = staffMember.FullName,
            Date = date.Date,
            DayOfWeek = projectDayOfWeek,
            DayName = dayName,
            SlotDurationMinutes = slotDuration,
            Slots = new List<AvailableSlotDto>()
        };

        var workingHour = await _context.StaffWorkingHours
            .FirstOrDefaultAsync(w =>
                w.StaffMemberId == staffMemberId &&
                w.DayOfWeek == projectDayOfWeek);

        if (workingHour == null || !workingHour.IsWorkingDay)
            return baseResponse;

        if (!TimeSpan.TryParse(workingHour.StartTime, out var startTime) ||
            !TimeSpan.TryParse(workingHour.EndTime, out var endTime))
            return baseResponse;

        if (endTime <= startTime)
            return baseResponse;

        TimeSpan? breakStart = null;
        TimeSpan? breakEnd = null;

        if (!string.IsNullOrWhiteSpace(workingHour.BreakStartTime) &&
            !string.IsNullOrWhiteSpace(workingHour.BreakEndTime) &&
            TimeSpan.TryParse(workingHour.BreakStartTime, out var parsedBreakStart) &&
            TimeSpan.TryParse(workingHour.BreakEndTime, out var parsedBreakEnd) &&
            parsedBreakEnd > parsedBreakStart)
        {
            breakStart = parsedBreakStart;
            breakEnd = parsedBreakEnd;
        }

        var blockedTimes = await GetBlockedTimesAsync(staffMemberId, date);

        var slots = new List<AvailableSlotDto>();
        var current = startTime;
        var duration = TimeSpan.FromMinutes(slotDuration);

        while (current < endTime)
        {
            if (breakStart.HasValue && breakEnd.HasValue &&
                current >= breakStart.Value && current < breakEnd.Value)
            {
                current += duration;
                continue;
            }

            var timeString = $"{current.Hours:D2}:{current.Minutes:D2}";

            if (!blockedTimes.Contains(timeString))
            {
                slots.Add(new AvailableSlotDto { Time = timeString, IsAvailable = true });
            }

            current += duration;
        }

        baseResponse.Slots = slots;
        return baseResponse;
    }

    private async Task<HashSet<string>> GetBlockedTimesAsync(int staffMemberId, DateTime date)
    {
        var dateOnly = date.Date;

        var bookedTimes = await _context.Appointments
            .Where(a =>
                a.StaffMemberId == staffMemberId &&
                a.RequestedDate.Date == dateOnly &&
                (a.Status == AppointmentStatuses.Pending || a.Status == AppointmentStatuses.Confirmed))
            .Select(a => a.RequestedTime)
            .ToListAsync();

        var blocked = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var time in bookedTimes)
        {
            if (TimeSpan.TryParse(time, out var parsed))
                blocked.Add($"{parsed.Hours:D2}:{parsed.Minutes:D2}");
        }

        return blocked;
    }

    private static int ToProjectDayOfWeek(DayOfWeek dotNetDayOfWeek) => dotNetDayOfWeek switch
    {
        DayOfWeek.Monday => 1,
        DayOfWeek.Tuesday => 2,
        DayOfWeek.Wednesday => 3,
        DayOfWeek.Thursday => 4,
        DayOfWeek.Friday => 5,
        DayOfWeek.Saturday => 6,
        DayOfWeek.Sunday => 7,
        _ => throw new ArgumentOutOfRangeException(nameof(dotNetDayOfWeek))
    };

    private static string GetDayName(int projectDayOfWeek) => projectDayOfWeek switch
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
