using BusinessKit.Application.ContactMessages;
using BusinessKit.Application.ContactMessages.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace BusinessKit.Api.Controllers;

[ApiController]
[Route("api/contact-messages")]
public class ContactMessagesController : ControllerBase
{
    private readonly IContactMessageService _contactMessageService;

    public ContactMessagesController(IContactMessageService contactMessageService)
    {
        _contactMessageService = contactMessageService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateContactMessageDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _contactMessageService.CreateAsync(dto, ipAddress);

        return StatusCode(201, result);
    }
}
