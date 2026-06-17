using BusinessKit.Application.Staff;
using Microsoft.AspNetCore.Mvc;

namespace BusinessKit.Api.Controllers;

[ApiController]
[Route("api/staff")]
[Tags("Staff (Public)")]
public class StaffController : ControllerBase
{
    private readonly IStaffService _staffService;

    public StaffController(IStaffService staffService)
    {
        _staffService = staffService;
    }

    [HttpGet]
    public async Task<IActionResult> GetActive()
    {
        var staff = await _staffService.GetActiveStaffAsync();
        return Ok(staff);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var member = await _staffService.GetActiveStaffBySlugAsync(slug);
        if (member == null)
            return NotFound(new { message = $"Staff member with slug '{slug}' was not found." });

        return Ok(member);
    }
}
