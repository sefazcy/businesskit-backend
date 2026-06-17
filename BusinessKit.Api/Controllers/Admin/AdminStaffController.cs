using BusinessKit.Application.Exceptions;
using BusinessKit.Application.Staff;
using BusinessKit.Application.Staff.Dtos;
using BusinessKit.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusinessKit.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/staff")]
[Authorize(Roles = Roles.Admin)]
[Tags("Staff (Admin)")]
public class AdminStaffController : ControllerBase
{
    private readonly IStaffService _staffService;

    public AdminStaffController(IStaffService staffService)
    {
        _staffService = staffService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive)
    {
        var staff = await _staffService.GetAllStaffAsync(isActive);
        return Ok(staff);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var member = await _staffService.GetStaffByIdAsync(id);
        if (member == null)
            return NotFound(new { message = $"Staff member with id {id} was not found." });

        return Ok(member);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStaffMemberDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var member = await _staffService.CreateStaffAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = member.Id }, member);
        }
        catch (DuplicateStaffSlugException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateStaffMemberDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var member = await _staffService.UpdateStaffAsync(id, dto);
            if (member == null)
                return NotFound(new { message = $"Staff member with id {id} was not found." });

            return Ok(member);
        }
        catch (DuplicateStaffSlugException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:int}/toggle-active")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var member = await _staffService.ToggleActiveAsync(id);
        if (member == null)
            return NotFound(new { message = $"Staff member with id {id} was not found." });

        return Ok(member);
    }
}
