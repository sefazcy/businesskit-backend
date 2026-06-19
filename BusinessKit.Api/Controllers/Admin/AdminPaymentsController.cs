using BusinessKit.Application.Payments;
using BusinessKit.Application.Payments.Dtos;
using BusinessKit.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusinessKit.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/payments")]
[Authorize(Roles = Roles.Admin)]
[Tags("Payments (Admin)")]
public class AdminPaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public AdminPaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status,
        [FromQuery] int? appointmentId,
        [FromQuery] int take = 50)
    {
        var payments = await _paymentService.GetAllAsync(status, appointmentId, take);
        return Ok(payments);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var payment = await _paymentService.GetByIdAsync(id);
        if (payment == null)
            return NotFound(new { message = $"Payment with id {id} was not found." });

        return Ok(payment);
    }

    [HttpPatch("{id:int}/mark-paid")]
    public async Task<IActionResult> MarkPaid(int id)
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

    [HttpPatch("{id:int}/mark-failed")]
    public async Task<IActionResult> MarkFailed(int id, [FromBody] MarkPaymentFailedDto? dto)
    {
        try
        {
            var payment = await _paymentService.MarkFailedAsync(id, dto ?? new MarkPaymentFailedDto());
            if (payment == null)
                return NotFound(new { message = $"Payment with id {id} was not found." });

            return Ok(payment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:int}/mark-refunded")]
    public async Task<IActionResult> MarkRefunded(int id, [FromBody] MarkPaymentRefundedDto? dto)
    {
        try
        {
            var payment = await _paymentService.MarkRefundedAsync(id, dto ?? new MarkPaymentRefundedDto());
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
