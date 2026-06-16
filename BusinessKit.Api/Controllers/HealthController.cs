using Microsoft.AspNetCore.Mvc;

namespace BusinessKit.Api.Controllers;

[ApiController]
[Route("api/health")]
[Tags("Health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}
