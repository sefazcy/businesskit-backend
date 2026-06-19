using BusinessKit.Application.Notifications;
using BusinessKit.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusinessKit.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/notifications")]
[Authorize(Roles = Roles.Admin)]
[Tags("Notifications (Admin)")]
public class AdminNotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public AdminNotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool? unreadOnly,
        [FromQuery] string? type,
        [FromQuery] int take = 50)
    {
        var notifications = await _notificationService.GetAllAsync(unreadOnly, type, take);
        return Ok(notifications);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var summary = await _notificationService.GetSummaryAsync();
        return Ok(summary);
    }

    [HttpPatch("{id:int}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        var notification = await _notificationService.MarkAsReadAsync(id);
        if (notification == null)
            return NotFound(new { message = $"Notification with id {id} was not found." });

        return Ok(notification);
    }

    [HttpPatch("{id:int}/unread")]
    public async Task<IActionResult> MarkUnread(int id)
    {
        var notification = await _notificationService.MarkAsUnreadAsync(id);
        if (notification == null)
            return NotFound(new { message = $"Notification with id {id} was not found." });

        return Ok(notification);
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        var count = await _notificationService.MarkAllAsReadAsync();
        return Ok(new { markedCount = count });
    }
}
