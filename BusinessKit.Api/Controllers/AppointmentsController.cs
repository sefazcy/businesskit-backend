using BusinessKit.Application.Appointments;
using BusinessKit.Application.Appointments.Dtos;
using BusinessKit.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace BusinessKit.Api.Controllers;

[ApiController]
[Route("api/appointments")]
[Tags("Appointments (Public)")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var appointment = await _appointmentService.CreateAsync(dto);
            return CreatedAtAction(nameof(Create), new { id = appointment.Id }, appointment);
        }
        catch (InvalidAppointmentReferenceException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidAppointmentTimeException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (AppointmentTimeUnavailableException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
