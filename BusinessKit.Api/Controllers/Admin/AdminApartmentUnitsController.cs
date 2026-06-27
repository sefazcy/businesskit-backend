using BusinessKit.Application.ApartmentManagement;
using BusinessKit.Application.ApartmentManagement.Dtos;
using BusinessKit.Application.Exceptions;
using BusinessKit.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusinessKit.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/apartment-units")]
[Authorize(Roles = Roles.Admin)]
[Tags("Apartment Units (Admin)")]
public class AdminApartmentUnitsController : ControllerBase
{
    private readonly IApartmentUnitService _apartmentUnitService;
    private readonly IResidentService _residentService;

    public AdminApartmentUnitsController(
        IApartmentUnitService apartmentUnitService,
        IResidentService residentService)
    {
        _apartmentUnitService = apartmentUnitService;
        _residentService = residentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ApartmentUnitListQuery query)
    {
        var units = await _apartmentUnitService.GetAllAsync(query);
        return Ok(units);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var unit = await _apartmentUnitService.GetByIdAsync(id);
        if (unit == null)
            return NotFound(new { message = $"Apartment unit with id {id} was not found." });

        return Ok(unit);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateApartmentUnitRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var unit = await _apartmentUnitService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = unit.Id }, unit);
        }
        catch (InvalidUnitTypeException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (DuplicateApartmentUnitException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateApartmentUnitRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var unit = await _apartmentUnitService.UpdateAsync(id, request);
            if (unit == null)
                return NotFound(new { message = $"Apartment unit with id {id} was not found." });

            return Ok(unit);
        }
        catch (InvalidUnitTypeException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (DuplicateApartmentUnitException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:int}/toggle-active")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var unit = await _apartmentUnitService.ToggleActiveAsync(id);
        if (unit == null)
            return NotFound(new { message = $"Apartment unit with id {id} was not found." });

        return Ok(unit);
    }

    [HttpGet("{id:int}/residents")]
    public async Task<IActionResult> GetResidents(int id)
    {
        var residents = await _residentService.GetByUnitIdAsync(id);
        return Ok(residents);
    }
}
