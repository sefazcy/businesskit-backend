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
        [FromQuery] DateTime? date)
    {
        try
        {
            var appointments = await _appointmentService.GetAllAsync(status, staffMemberId, businessServiceId, date);
            return Ok(appointments);
        }
        catch (InvalidAppointmentStatusException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
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
