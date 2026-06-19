using BusinessKit.Application.Payments;
using BusinessKit.Application.Payments.Dtos;
using BusinessKit.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusinessKit.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/appointments/{appointmentId:int}/payments")]
[Authorize(Roles = Roles.Admin)]
[Tags("Payments (Admin)")]
public class AdminAppointmentPaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public AdminAppointmentPaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetByAppointmentId(int appointmentId)
    {
        var payments = await _paymentService.GetByAppointmentIdAsync(appointmentId);
        return Ok(payments);
    }

    [HttpPost]
    public async Task<IActionResult> CreateForAppointment(int appointmentId, [FromBody] CreateAdminPaymentDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var payment = await _paymentService.CreateForAppointmentAsync(appointmentId, dto);
            if (payment == null)
                return NotFound(new { message = $"Appointment with id {appointmentId} was not found." });

            return Ok(payment);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
