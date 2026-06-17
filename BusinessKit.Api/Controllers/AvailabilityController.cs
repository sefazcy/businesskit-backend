using BusinessKit.Application.Availability;
using BusinessKit.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace BusinessKit.Api.Controllers;

[ApiController]
[Route("api/availability")]
[Tags("Availability (Public)")]
public class AvailabilityController : ControllerBase
{
    private readonly IAvailabilityService _service;

    public AvailabilityController(IAvailabilityService service)
    {
        _service = service;
    }

    [HttpGet("slots")]
    public async Task<IActionResult> GetSlots(
        [FromQuery] int staffMemberId,
        [FromQuery] DateTime date,
        [FromQuery] int? businessServiceId)
    {
        try
        {
            var result = await _service.GetAvailableSlotsAsync(staffMemberId, date, businessServiceId);
            return Ok(result);
        }
        catch (InvalidAvailabilityReferenceException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
