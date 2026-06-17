using BusinessKit.Application.Exceptions;
using BusinessKit.Application.StaffWorkingHours;
using BusinessKit.Application.StaffWorkingHours.Dtos;
using BusinessKit.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusinessKit.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/staff-working-hours")]
[Authorize(Roles = Roles.Admin)]
[Tags("Staff Working Hours (Admin)")]
public class AdminStaffWorkingHoursController : ControllerBase
{
    private readonly IStaffWorkingHourService _service;

    public AdminStaffWorkingHoursController(IStaffWorkingHourService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? staffMemberId)
    {
        var entries = await _service.GetAllAsync(staffMemberId);
        return Ok(entries);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var entry = await _service.GetByIdAsync(id);
        if (entry == null)
            return NotFound(new { message = $"Staff working hour with id {id} was not found." });

        return Ok(entry);
    }

    [HttpGet("/api/admin/staff/{staffMemberId:int}/working-hours")]
    public async Task<IActionResult> GetByStaffMember(int staffMemberId)
    {
        try
        {
            var entries = await _service.GetByStaffMemberIdAsync(staffMemberId);
            return Ok(entries);
        }
        catch (InvalidStaffWorkingHourReferenceException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStaffWorkingHourDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var entry = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = entry.Id }, entry);
        }
        catch (InvalidStaffWorkingHourReferenceException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidStaffWorkingHourException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (DuplicateStaffWorkingHourException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateStaffWorkingHourDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var entry = await _service.UpdateAsync(id, dto);
            if (entry == null)
                return NotFound(new { message = $"Staff working hour with id {id} was not found." });

            return Ok(entry);
        }
        catch (InvalidStaffWorkingHourException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (DuplicateStaffWorkingHourException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
