using BusinessKit.Application.ApartmentManagement;
using BusinessKit.Application.ApartmentManagement.Dtos;
using BusinessKit.Application.Exceptions;
using BusinessKit.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusinessKit.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/residents")]
[Authorize(Roles = Roles.Admin)]
[Tags("Residents (Admin)")]
public class AdminResidentsController : ControllerBase
{
    private readonly IResidentService _residentService;

    public AdminResidentsController(IResidentService residentService)
    {
        _residentService = residentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ResidentListQuery query)
    {
        var residents = await _residentService.GetAllAsync(query);
        return Ok(residents);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var resident = await _residentService.GetByIdAsync(id);
        if (resident == null)
            return NotFound(new { message = $"Resident with id {id} was not found." });

        return Ok(resident);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateResidentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var resident = await _residentService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = resident.Id }, resident);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidResidentRoleException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateResidentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var resident = await _residentService.UpdateAsync(id, request);
            if (resident == null)
                return NotFound(new { message = $"Resident with id {id} was not found." });

            return Ok(resident);
        }
        catch (InvalidResidentRoleException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:int}/toggle-active")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var resident = await _residentService.ToggleActiveAsync(id);
        if (resident == null)
            return NotFound(new { message = $"Resident with id {id} was not found." });

        return Ok(resident);
    }
}
