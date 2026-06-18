using BusinessKit.Application.Appointments;
using BusinessKit.Application.Appointments.Dtos;
using BusinessKit.Application.Exceptions;
using BusinessKit.Domain.Entities;
using BusinessKit.Infrastructure.Data;
using BusinessKit.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace BusinessKit.Infrastructure.Appointments;

public class AppointmentService : IAppointmentService
{
    private const int DefaultSlotDurationMinutes = 30;

    private readonly AppDbContext _context;

    public AppointmentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AppointmentDto> CreateAsync(CreateAppointmentRequestDto dto)
    {
        await ValidateStaffMemberAsync(dto.StaffMemberId);
        await ValidateBusinessServiceAsync(dto.BusinessServiceId);

        if (dto.StaffMemberId.HasValue)
            await ValidateAppointmentAvailabilityAsync(dto.StaffMemberId.Value, dto.RequestedDate, dto.RequestedTime, dto.BusinessServiceId);

        var appointment = new Appointment
        {
            CustomerFullName = dto.CustomerFullName,
            CustomerEmail = dto.CustomerEmail,
            CustomerPhone = dto.CustomerPhone,
            StaffMemberId = dto.StaffMemberId,
            BusinessServiceId = dto.BusinessServiceId,
            RequestedDate = dto.RequestedDate,
            RequestedTime = dto.RequestedTime,
            Note = dto.Note,
            Status = AppointmentStatuses.Pending,
            AdminNote = null
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        await _context.Entry(appointment).Reference(a => a.StaffMember).LoadAsync();
        await _context.Entry(appointment).Reference(a => a.BusinessService).LoadAsync();

        return MapToDto(appointment);
    }

    public async Task<List<AppointmentDto>> GetAllAsync(
        string? status,
        int? staffMemberId,
        int? businessServiceId,
        DateTime? date,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? customerName = null,
        string? customerEmail = null,
        string? customerPhone = null)
    {
        if (status != null && !AppointmentStatuses.IsValid(status))
            throw new InvalidAppointmentStatusException(status);

        var query = _context.Appointments
            .Include(a => a.StaffMember)
            .Include(a => a.BusinessService)
            .AsQueryable();

        if (status != null)
            query = query.Where(a => a.Status == status);

        if (staffMemberId.HasValue)
            query = query.Where(a => a.StaffMemberId == staffMemberId.Value);

        if (businessServiceId.HasValue)
            query = query.Where(a => a.BusinessServiceId == businessServiceId.Value);

        if (date.HasValue)
        {
            var dayStart = date.Value.Date;
            var dayEnd = dayStart.AddDays(1);
            query = query.Where(a => a.RequestedDate >= dayStart && a.RequestedDate < dayEnd);
        }
        else
        {
            if (startDate.HasValue)
                query = query.Where(a => a.RequestedDate >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(a => a.RequestedDate < endDate.Value.Date.AddDays(1));
        }

        var nameTerm = customerName?.Trim();
        if (!string.IsNullOrWhiteSpace(nameTerm))
            query = query.Where(a => EF.Functions.Like(a.CustomerFullName, $"%{nameTerm}%"));

        var emailTerm = customerEmail?.Trim();
        if (!string.IsNullOrWhiteSpace(emailTerm))
            query = query.Where(a => EF.Functions.Like(a.CustomerEmail ?? "", $"%{emailTerm}%"));

        var phoneTerm = customerPhone?.Trim();
        if (!string.IsNullOrWhiteSpace(phoneTerm))
            query = query.Where(a => EF.Functions.Like(a.CustomerPhone, $"%{phoneTerm}%"));

        var appointments = await query
            .OrderBy(a => a.RequestedDate)
            .ThenBy(a => a.RequestedTime)
            .ThenBy(a => a.Id)
            .ToListAsync();

        return appointments.Select(MapToDto).ToList();
    }

    public async Task<AppointmentDto?> GetByIdAsync(int id)
    {
        var appointment = await _context.Appointments
            .Include(a => a.StaffMember)
            .Include(a => a.BusinessService)
            .FirstOrDefaultAsync(a => a.Id == id);

        return appointment == null ? null : MapToDto(appointment);
    }

    public async Task<AppointmentDto?> UpdateStatusAsync(int id, UpdateAppointmentStatusDto dto)
    {
        if (!AppointmentStatuses.IsValid(dto.Status))
            throw new InvalidAppointmentStatusException(dto.Status);

        var appointment = await _context.Appointments
            .Include(a => a.StaffMember)
            .Include(a => a.BusinessService)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null)
            return null;

        appointment.Status = dto.Status;
        appointment.AdminNote = dto.AdminNote;

        await _context.SaveChangesAsync();

        return MapToDto(appointment);
    }

    public async Task<AppointmentDto?> UpdateAsync(int id, UpdateAppointmentDto dto)
    {
        if (!AppointmentStatuses.IsValid(dto.Status))
            throw new InvalidAppointmentStatusException(dto.Status);

        await ValidateStaffMemberAsync(dto.StaffMemberId);
        await ValidateBusinessServiceAsync(dto.BusinessServiceId);

        var appointment = await _context.Appointments
            .Include(a => a.StaffMember)
            .Include(a => a.BusinessService)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null)
            return null;

        appointment.CustomerFullName = dto.CustomerFullName;
        appointment.CustomerEmail = dto.CustomerEmail;
        appointment.CustomerPhone = dto.CustomerPhone;
        appointment.StaffMemberId = dto.StaffMemberId;
        appointment.BusinessServiceId = dto.BusinessServiceId;
        appointment.RequestedDate = dto.RequestedDate;
        appointment.RequestedTime = dto.RequestedTime;
        appointment.Note = dto.Note;
        appointment.Status = dto.Status;
        appointment.AdminNote = dto.AdminNote;

        await _context.SaveChangesAsync();

        // Reload nav properties because FK values may have changed
        await _context.Entry(appointment).Reference(a => a.StaffMember).LoadAsync();
        await _context.Entry(appointment).Reference(a => a.BusinessService).LoadAsync();

        return MapToDto(appointment);
    }

    public async Task<List<AppointmentDto>> GetTodayAsync(string? status, int? staffMemberId, int? businessServiceId)
    {
        if (status != null && !AppointmentStatuses.IsValid(status))
            throw new InvalidAppointmentStatusException(status);

        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var query = _context.Appointments
            .Include(a => a.StaffMember)
            .Include(a => a.BusinessService)
            .Where(a => a.RequestedDate >= today && a.RequestedDate < tomorrow)
            .AsQueryable();

        if (status != null)
            query = query.Where(a => a.Status == status);

        if (staffMemberId.HasValue)
            query = query.Where(a => a.StaffMemberId == staffMemberId.Value);

        if (businessServiceId.HasValue)
            query = query.Where(a => a.BusinessServiceId == businessServiceId.Value);

        var appointments = await query
            .OrderBy(a => a.RequestedDate)
            .ThenBy(a => a.RequestedTime)
            .ThenBy(a => a.Id)
            .ToListAsync();

        return appointments.Select(MapToDto).ToList();
    }

    public async Task<List<AppointmentDto>> GetUpcomingAsync(string? status, int? staffMemberId, int? businessServiceId, int days)
    {
        if (status != null && !AppointmentStatuses.IsValid(status))
            throw new InvalidAppointmentStatusException(status);

        var today = DateTime.UtcNow.Date;
        var rangeEnd = today.AddDays(days + 1);

        var query = _context.Appointments
            .Include(a => a.StaffMember)
            .Include(a => a.BusinessService)
            .Where(a => a.RequestedDate >= today && a.RequestedDate < rangeEnd)
            .AsQueryable();

        if (status != null)
            query = query.Where(a => a.Status == status);

        if (staffMemberId.HasValue)
            query = query.Where(a => a.StaffMemberId == staffMemberId.Value);

        if (businessServiceId.HasValue)
            query = query.Where(a => a.BusinessServiceId == businessServiceId.Value);

        var appointments = await query
            .OrderBy(a => a.RequestedDate)
            .ThenBy(a => a.RequestedTime)
            .ThenBy(a => a.Id)
            .ToListAsync();

        return appointments.Select(MapToDto).ToList();
    }

    public async Task<AppointmentStatsDto> GetStatsAsync(int? staffMemberId, int? businessServiceId, DateTime? startDate, DateTime? endDate)
    {
        var query = _context.Appointments.AsQueryable();

        if (staffMemberId.HasValue)
            query = query.Where(a => a.StaffMemberId == staffMemberId.Value);

        if (businessServiceId.HasValue)
            query = query.Where(a => a.BusinessServiceId == businessServiceId.Value);

        if (startDate.HasValue)
            query = query.Where(a => a.RequestedDate >= startDate.Value.Date);

        if (endDate.HasValue)
            query = query.Where(a => a.RequestedDate < endDate.Value.Date.AddDays(1));

        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        var upcoming7End = today.AddDays(8);

        var total = await query.CountAsync();
        var pending = await query.CountAsync(a => a.Status == AppointmentStatuses.Pending);
        var confirmed = await query.CountAsync(a => a.Status == AppointmentStatuses.Confirmed);
        var cancelled = await query.CountAsync(a => a.Status == AppointmentStatuses.Cancelled);
        var completed = await query.CountAsync(a => a.Status == AppointmentStatuses.Completed);
        var todayCount = await query.CountAsync(a => a.RequestedDate >= today && a.RequestedDate < tomorrow);
        var upcoming7Count = await query.CountAsync(a => a.RequestedDate >= today && a.RequestedDate < upcoming7End);

        return new AppointmentStatsDto
        {
            TotalAppointments = total,
            PendingCount = pending,
            ConfirmedCount = confirmed,
            CancelledCount = cancelled,
            CompletedCount = completed,
            TodayCount = todayCount,
            Upcoming7DaysCount = upcoming7Count
        };
    }

    private async Task ValidateAppointmentAvailabilityAsync(
        int staffMemberId,
        DateTime requestedDate,
        string requestedTime,
        int? businessServiceId)
    {
        if (!TimeSpan.TryParse(requestedTime, out var requestedTs))
            throw new InvalidAppointmentTimeException(
                $"RequestedTime '{requestedTime}' is not a valid time. Use format HH:mm, e.g. '09:00'.");

        var normalizedRequest = $"{requestedTs.Hours:D2}:{requestedTs.Minutes:D2}";

        var durationMinutes = DefaultSlotDurationMinutes;
        if (businessServiceId.HasValue)
        {
            var service = await _context.BusinessServices.FindAsync(businessServiceId.Value);
            if (service != null && service.DurationMinutes > 0)
                durationMinutes = service.DurationMinutes;
        }

        var duration = TimeSpan.FromMinutes(durationMinutes);
        var requestedEnd = requestedTs + duration;

        var projectDay = ToProjectDayOfWeek(requestedDate.DayOfWeek);

        var workingHour = await _context.StaffWorkingHours
            .FirstOrDefaultAsync(w => w.StaffMemberId == staffMemberId && w.DayOfWeek == projectDay);

        if (workingHour == null || !workingHour.IsWorkingDay)
            throw new InvalidAppointmentTimeException(
                $"Staff member {staffMemberId} does not work on {requestedDate:dddd, yyyy-MM-dd}.");

        if (!TimeSpan.TryParse(workingHour.StartTime, out var startTs) ||
            !TimeSpan.TryParse(workingHour.EndTime, out var endTs))
            throw new InvalidAppointmentTimeException(
                "Staff working hours are not configured correctly for this day.");

        if (requestedTs < startTs || requestedEnd > endTs)
            throw new InvalidAppointmentTimeException(
                $"Requested time '{normalizedRequest}' is outside working hours ({workingHour.StartTime}–{workingHour.EndTime}).");

        if (!string.IsNullOrWhiteSpace(workingHour.BreakStartTime) &&
            !string.IsNullOrWhiteSpace(workingHour.BreakEndTime) &&
            TimeSpan.TryParse(workingHour.BreakStartTime, out var breakStartTs) &&
            TimeSpan.TryParse(workingHour.BreakEndTime, out var breakEndTs) &&
            breakEndTs > breakStartTs)
        {
            if (requestedTs < breakEndTs && requestedEnd > breakStartTs)
                throw new InvalidAppointmentTimeException(
                    $"Requested time '{normalizedRequest}' overlaps the break period ({workingHour.BreakStartTime}–{workingHour.BreakEndTime}).");
        }

        var dateOnly = requestedDate.Date;

        var existingAppointments = await _context.Appointments
            .Include(a => a.BusinessService)
            .Where(a =>
                a.StaffMemberId == staffMemberId &&
                a.RequestedDate.Date == dateOnly &&
                (a.Status == AppointmentStatuses.Pending || a.Status == AppointmentStatuses.Confirmed))
            .ToListAsync();

        foreach (var existing in existingAppointments)
        {
            if (!TimeSpan.TryParse(existing.RequestedTime, out var existingStart))
                continue;

            var existingDuration = existing.BusinessService?.DurationMinutes > 0
                ? existing.BusinessService.DurationMinutes
                : DefaultSlotDurationMinutes;

            var existingEnd = existingStart + TimeSpan.FromMinutes(existingDuration);

            if (requestedTs < existingEnd && requestedEnd > existingStart)
                throw new AppointmentTimeUnavailableException(normalizedRequest);
        }
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

    private async Task ValidateStaffMemberAsync(int? staffMemberId)
    {
        if (!staffMemberId.HasValue)
            return;

        var exists = await _context.StaffMembers.AnyAsync(s => s.Id == staffMemberId.Value);
        if (!exists)
            throw new InvalidAppointmentReferenceException(
                $"Staff member with ID {staffMemberId.Value} was not found.");
    }

    private async Task ValidateBusinessServiceAsync(int? businessServiceId)
    {
        if (!businessServiceId.HasValue)
            return;

        var exists = await _context.BusinessServices.AnyAsync(s => s.Id == businessServiceId.Value);
        if (!exists)
            throw new InvalidAppointmentReferenceException(
                $"Business service with ID {businessServiceId.Value} was not found.");
    }

    private static AppointmentDto MapToDto(Appointment a) => new()
    {
        Id = a.Id,
        CustomerFullName = a.CustomerFullName,
        CustomerEmail = a.CustomerEmail,
        CustomerPhone = a.CustomerPhone,
        StaffMemberId = a.StaffMemberId,
        StaffMemberName = a.StaffMember?.FullName,
        BusinessServiceId = a.BusinessServiceId,
        BusinessServiceTitle = a.BusinessService?.Title,
        RequestedDate = a.RequestedDate,
        RequestedTime = a.RequestedTime,
        Note = a.Note,
        Status = a.Status,
        AdminNote = a.AdminNote,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt
    };
}
