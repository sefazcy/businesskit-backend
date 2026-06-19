using BusinessKit.Application.Payments;
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

    [HttpGet("{id:int}/status")]
    public async Task<IActionResult> GetStatus(int id)
    {
        var status = await _paymentService.GetPublicStatusAsync(id);
        if (status == null)
            return NotFound(new { message = $"Payment with id {id} was not found." });

        return Ok(status);
    }
}
