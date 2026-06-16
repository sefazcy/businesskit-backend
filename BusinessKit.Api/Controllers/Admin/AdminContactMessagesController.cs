using BusinessKit.Application.ContactMessages;
using BusinessKit.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusinessKit.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/contact-messages")]
[Authorize(Roles = Roles.Admin)]
[Tags("Contact Messages (Admin)")]
public class AdminContactMessagesController : ControllerBase
{
    private readonly IContactMessageService _contactMessageService;

    public AdminContactMessagesController(IContactMessageService contactMessageService)
    {
        _contactMessageService = contactMessageService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? unreadOnly, [FromQuery] bool? archivedOnly)
    {
        var messages = await _contactMessageService.GetAllAsync(unreadOnly, archivedOnly);
        return Ok(messages);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var message = await _contactMessageService.GetByIdAsync(id);
        if (message == null)
            return NotFound(new { message = $"Contact message with id {id} was not found." });

        return Ok(message);
    }

    [HttpPatch("{id:int}/mark-read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        var message = await _contactMessageService.MarkReadAsync(id);
        if (message == null)
            return NotFound(new { message = $"Contact message with id {id} was not found." });

        return Ok(message);
    }

    [HttpPatch("{id:int}/mark-unread")]
    public async Task<IActionResult> MarkUnread(int id)
    {
        var message = await _contactMessageService.MarkUnreadAsync(id);
        if (message == null)
            return NotFound(new { message = $"Contact message with id {id} was not found." });

        return Ok(message);
    }

    [HttpPatch("{id:int}/mark-replied")]
    public async Task<IActionResult> MarkReplied(int id)
    {
        var message = await _contactMessageService.MarkRepliedAsync(id);
        if (message == null)
            return NotFound(new { message = $"Contact message with id {id} was not found." });

        return Ok(message);
    }

    [HttpPatch("{id:int}/archive")]
    public async Task<IActionResult> Archive(int id)
    {
        var message = await _contactMessageService.ArchiveAsync(id);
        if (message == null)
            return NotFound(new { message = $"Contact message with id {id} was not found." });

        return Ok(message);
    }

    [HttpPatch("{id:int}/unarchive")]
    public async Task<IActionResult> Unarchive(int id)
    {
        var message = await _contactMessageService.UnarchiveAsync(id);
        if (message == null)
            return NotFound(new { message = $"Contact message with id {id} was not found." });

        return Ok(message);
    }
}
