using BusinessKit.Application.BusinessSettings;
using BusinessKit.Application.BusinessSettings.Dtos;
using BusinessKit.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusinessKit.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/business-settings")]
[Authorize(Roles = Roles.Admin)]
public class AdminBusinessSettingsController : ControllerBase
{
    private readonly IBusinessSettingsService _businessSettingsService;

    public AdminBusinessSettingsController(IBusinessSettingsService businessSettingsService)
    {
        _businessSettingsService = businessSettingsService;
    }

    [HttpPut]
    public async Task<IActionResult> Upsert([FromBody] UpdateBusinessSettingsDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var settings = await _businessSettingsService.UpsertAsync(dto);
        return Ok(settings);
    }
}
