using BusinessKit.Application.Common;
using BusinessKit.Application.Payments;
using BusinessKit.Application.Payments.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BusinessKit.Api.Controllers;

[ApiController]
[Route("api/payments")]
[Tags("Payments (Public)")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IWebHostEnvironment _env;
    private readonly FrontendUrlOptions _frontendUrls;

    public PaymentsController(
        IPaymentService paymentService,
        IWebHostEnvironment env,
        IOptions<FrontendUrlOptions> frontendUrlOptions)
    {
        _paymentService = paymentService;
        _env = env;
        _frontendUrls = frontendUrlOptions.Value;
    }

    /// <summary>
    /// Returns the minimal public status of a payment, looked up by its public token
    /// (never by numeric id, so payments cannot be enumerated).
    /// No sensitive or internal fields are exposed.
    /// </summary>
    [HttpGet("{paymentToken}/status")]
    public async Task<IActionResult> GetStatus(string paymentToken)
    {
        var status = await _paymentService.GetPublicStatusAsync(paymentToken);
        if (status == null)
            return NotFound(new { message = "Payment was not found." });

        return Ok(status);
    }

    /// <summary>
    /// Creates or returns a pending checkout session for an appointment, identified by
    /// the appointment's public token (returned when the appointment was created).
    /// Calling this multiple times while a Pending payment exists returns the
    /// existing payment (idempotent).
    /// </summary>
    [HttpPost("checkout")]
    public async Task<IActionResult> CreateCheckout([FromBody] CreatePaymentCheckoutRequestDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _paymentService.CreatePublicCheckoutAsync(dto.AppointmentToken);
            if (result == null)
                return NotFound(new { message = "Appointment was not found." });

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Receives the Iyzico payment gateway callback after a checkout attempt.
    /// Accepts application/x-www-form-urlencoded (the format Iyzico actually posts,
    /// via the customer's browser) and application/json (Swagger / manual testing).
    /// Verifies the payment via the Iyzico API and updates payment status.
    /// Only provider-side verification can mark a payment Paid.
    ///
    /// Form posts come from the customer's browser, so they are answered with a
    /// 303 redirect to the public payment status page. JSON requests still get
    /// the JSON result for testing.
    /// </summary>
    [HttpPost("iyzico/callback")]
    public async Task<IActionResult> IyzicoCallback()
    {
        var isBrowserPost = Request.HasFormContentType;
        string? token;

        if (isBrowserPost)
        {
            token = (await Request.ReadFormAsync())["token"].FirstOrDefault();
        }
        else
        {
            var dto = await Request.ReadFromJsonAsync<IyzicoCallbackRequest>();
            token = dto?.Token;
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            if (isBrowserPost)
                return SeeOther(_frontendUrls.BuildPaymentStatusUrl(null));

            return BadRequest(new { message = "Token is required." });
        }

        var result = await _paymentService.HandleIyzicoCallbackAsync(new IyzicoCallbackRequest { Token = token });

        if (isBrowserPost)
            return SeeOther(_frontendUrls.BuildPaymentStatusUrl(result.PaymentToken));

        return Ok(result);
    }

    /// <summary>303 See Other: tells the browser to follow up the POST with a GET.</summary>
    private IActionResult SeeOther(string url)
    {
        Response.Headers.Location = url;
        return StatusCode(StatusCodes.Status303SeeOther);
    }

    /// <summary>
    /// [DEV ONLY — Development environment only]
    /// Simulates a successful payment by marking a Pending payment as Paid.
    /// Triggers the same notifications and email as the admin mark-paid action.
    /// Available ONLY when the application is running in the Development environment.
    /// Returns 404 in all other environments so the endpoint does not appear to exist.
    /// NOT for production use — no real payment provider is involved.
    /// </summary>
    [HttpPatch("{paymentToken}/simulate-paid")]
    public async Task<IActionResult> SimulatePaid(string paymentToken)
    {
        if (!_env.IsDevelopment())
            return NotFound();

        try
        {
            var id = await _paymentService.FindIdByPublicTokenAsync(paymentToken);
            var payment = id.HasValue ? await _paymentService.MarkPaidAsync(id.Value) : null;
            if (payment == null)
                return NotFound(new { message = "Payment was not found." });

            return Ok(payment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
