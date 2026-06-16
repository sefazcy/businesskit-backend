using BusinessKit.Application.Exceptions;
using BusinessKit.Application.ServiceCatalog;
using BusinessKit.Application.ServiceCatalog.Dtos;
using BusinessKit.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusinessKit.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/services")]
[Authorize(Roles = Roles.Admin)]
[Tags("Services (Admin)")]
public class AdminServicesController : ControllerBase
{
    private readonly IServiceCatalogService _serviceCatalogService;

    public AdminServicesController(IServiceCatalogService serviceCatalogService)
    {
        _serviceCatalogService = serviceCatalogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var services = await _serviceCatalogService.GetAllServicesAsync();
        return Ok(services);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var service = await _serviceCatalogService.GetServiceByIdAsync(id);
        if (service == null)
            return NotFound(new { message = $"Service with id {id} was not found." });

        return Ok(service);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateServiceDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var service = await _serviceCatalogService.CreateServiceAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = service.Id }, service);
        }
        catch (DuplicateSlugException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateServiceDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var service = await _serviceCatalogService.UpdateServiceAsync(id, dto);
            if (service == null)
                return NotFound(new { message = $"Service with id {id} was not found." });

            return Ok(service);
        }
        catch (DuplicateSlugException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:int}/toggle-active")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var service = await _serviceCatalogService.ToggleActiveAsync(id);
        if (service == null)
            return NotFound(new { message = $"Service with id {id} was not found." });

        return Ok(service);
    }
}
