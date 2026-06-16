using BusinessKit.Application.BusinessSettings;
using Microsoft.AspNetCore.Mvc;

namespace BusinessKit.Api.Controllers;

[ApiController]
[Route("api/business-settings")]
[Tags("Business Settings (Public)")]
public class BusinessSettingsController : ControllerBase
{
    private readonly IBusinessSettingsService _businessSettingsService;

    public BusinessSettingsController(IBusinessSettingsService businessSettingsService)
    {
        _businessSettingsService = businessSettingsService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var settings = await _businessSettingsService.GetAsync();
        if (settings == null)
            return NotFound(new { message = "Business settings have not been configured yet." });

        return Ok(settings);
    }
}
