using BusinessKit.Application.Appointments;
using BusinessKit.Application.Appointments.Dtos;
using BusinessKit.Application.Exceptions;
using BusinessKit.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusinessKit.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/appointments")]
[Authorize(Roles = Roles.Admin)]
[Tags("Appointments (Admin)")]
public class AdminAppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AdminAppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status,
        [FromQuery] int? staffMemberId,
        [FromQuery] int? businessServiceId,
        [FromQuery] DateTime? date,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] string? customerName,
        [FromQuery] string? customerEmail,
        [FromQuery] string? customerPhone)
    {
        if (date.HasValue && (startDate.HasValue || endDate.HasValue))
            return BadRequest(new { message = "Use 'date' for a single day or 'startDate'/'endDate' for a range, not both." });

        try
        {
            var appointments = await _appointmentService.GetAllAsync(status, staffMemberId, businessServiceId, date, startDate, endDate, customerName, customerEmail, customerPhone);
            return Ok(appointments);
        }
        catch (InvalidAppointmentStatusException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("today")]
    public async Task<IActionResult> GetToday(
        [FromQuery] string? status,
        [FromQuery] int? staffMemberId,
        [FromQuery] int? businessServiceId)
    {
        try
        {
            var appointments = await _appointmentService.GetTodayAsync(status, staffMemberId, businessServiceId);
            return Ok(appointments);
        }
        catch (InvalidAppointmentStatusException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("upcoming")]
    public async Task<IActionResult> GetUpcoming(
        [FromQuery] string? status,
        [FromQuery] int? staffMemberId,
        [FromQuery] int? businessServiceId,
        [FromQuery] int? days)
    {
        var resolvedDays = days ?? 7;

        if (resolvedDays <= 0)
            return BadRequest(new { message = "days must be greater than 0." });

        try
        {
            var appointments = await _appointmentService.GetUpcomingAsync(status, staffMemberId, businessServiceId, resolvedDays);
            return Ok(appointments);
        }
        catch (InvalidAppointmentStatusException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(
        [FromQuery] int? staffMemberId,
        [FromQuery] int? businessServiceId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var stats = await _appointmentService.GetStatsAsync(staffMemberId, businessServiceId, startDate, endDate);
        return Ok(stats);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var appointment = await _appointmentService.GetByIdAsync(id);
        if (appointment == null)
            return NotFound(new { message = $"Appointment with id {id} was not found." });

        return Ok(appointment);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateAppointmentStatusDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var appointment = await _appointmentService.UpdateStatusAsync(id, dto);
            if (appointment == null)
                return NotFound(new { message = $"Appointment with id {id} was not found." });

            return Ok(appointment);
        }
        catch (InvalidAppointmentStatusException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAppointmentDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var appointment = await _appointmentService.UpdateAsync(id, dto);
            if (appointment == null)
                return NotFound(new { message = $"Appointment with id {id} was not found." });

            return Ok(appointment);
        }
        catch (InvalidAppointmentStatusException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidAppointmentReferenceException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
