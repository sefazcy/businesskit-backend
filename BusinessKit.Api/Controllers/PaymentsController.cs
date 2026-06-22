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
    private readonly IWebHostEnvironment _env;

    public PaymentsController(IPaymentService paymentService, IWebHostEnvironment env)
    {
        _paymentService = paymentService;
        _env = env;
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
    /// Receives the Iyzico payment gateway callback after a checkout attempt.
    /// Accepts application/json (Swagger testing) and application/x-www-form-urlencoded
    /// (the format Iyzico actually posts). Verifies the payment via the Iyzico API
    /// and updates payment status. Only provider-side verification can mark a payment Paid.
    /// </summary>
    [HttpPost("iyzico/callback")]
    public async Task<IActionResult> IyzicoCallback()
    {
        string? token;

        if (Request.HasFormContentType)
        {
            token = Request.Form["token"].FirstOrDefault();
        }
        else
        {
            var dto = await Request.ReadFromJsonAsync<IyzicoCallbackRequest>();
            token = dto?.Token;
        }

        if (string.IsNullOrWhiteSpace(token))
            return BadRequest(new { message = "Token is required." });

        var result = await _paymentService.HandleIyzicoCallbackAsync(new IyzicoCallbackRequest { Token = token });
        return Ok(result);
    }

    /// <summary>
    /// [DEV ONLY — Development environment only]
    /// Simulates a successful payment by marking a Pending payment as Paid.
    /// Triggers the same notifications and email as the admin mark-paid action.
    /// Available ONLY when the application is running in the Development environment.
    /// Returns 404 in all other environments so the endpoint does not appear to exist.
    /// NOT for production use — no real payment provider is involved.
    /// </summary>
    [HttpPatch("{id:int}/simulate-paid")]
    public async Task<IActionResult> SimulatePaid(int id)
    {
        if (!_env.IsDevelopment())
            return NotFound();

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
