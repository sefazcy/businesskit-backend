using BusinessKit.Application.ServiceCatalog;
using Microsoft.AspNetCore.Mvc;

namespace BusinessKit.Api.Controllers;

[ApiController]
[Route("api/services")]
[Tags("Services (Public)")]
public class ServicesController : ControllerBase
{
    private readonly IServiceCatalogService _serviceCatalogService;

    public ServicesController(IServiceCatalogService serviceCatalogService)
    {
        _serviceCatalogService = serviceCatalogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetActive()
    {
        var services = await _serviceCatalogService.GetActiveServicesAsync();
        return Ok(services);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var service = await _serviceCatalogService.GetActiveServiceBySlugAsync(slug);
        if (service == null)
            return NotFound(new { message = $"Service with slug '{slug}' was not found." });

        return Ok(service);
    }
}
