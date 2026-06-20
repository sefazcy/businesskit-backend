using BusinessKit.Application.Payments;
using BusinessKit.Application.Payments.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace BusinessKit.Api.Controllers;

[ApiController]
[Route("api/payments")]
[Tags("Payments (Public)")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>
    /// Returns the minimal public status of a payment (id, status, paidAt).
    /// No sensitive or internal fields are exposed.
    /// </summary>
    [HttpGet("{id:int}/status")]
    public async Task<IActionResult> GetStatus(int id)
    {
        var status = await _paymentService.GetPublicStatusAsync(id);
        if (status == null)
            return NotFound(new { message = $"Payment with id {id} was not found." });

        return Ok(status);
    }

    /// <summary>
    /// Creates or returns a pending checkout session for an appointment.
    /// Calling this multiple times for the same appointment while a Pending payment
    /// exists returns the existing payment (idempotent). No external provider is
    /// called in this sprint — the checkoutUrl is a placeholder.
    /// </summary>
    [HttpPost("checkout")]
    public async Task<IActionResult> CreateCheckout([FromBody] CreatePaymentCheckoutRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _paymentService.CreatePublicCheckoutAsync(dto.AppointmentId);
            if (result == null)
                return NotFound(new { message = $"Appointment with id {dto.AppointmentId} was not found." });

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// [DEV ONLY] Simulates a successful payment by marking a Pending payment as Paid.
    /// Triggers the same notifications and email as admin mark-paid.
    /// This endpoint exists for development and manual testing only — do not expose
    /// in production without authentication and environment gating.
    /// </summary>
    [HttpPatch("{id:int}/simulate-paid")]
    public async Task<IActionResult> SimulatePaid(int id)
    {
        try
        {
            var payment = await _paymentService.MarkPaidAsync(id);
            if (payment == null)
                return NotFound(new { message = $"Payment with id {id} was not found." });

            return Ok(payment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
