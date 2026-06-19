using BusinessKit.Application.Notifications.Dtos;

namespace BusinessKit.Application.Notifications;

public interface INotificationService
{
    Task<List<NotificationDto>> GetAllAsync(bool? unreadOnly = null, string? type = null, int take = 50);
    Task<NotificationSummaryDto> GetSummaryAsync();
    Task<int> GetUnreadCountAsync();
    Task<NotificationDto?> MarkAsReadAsync(int id);
    Task<NotificationDto?> MarkAsUnreadAsync(int id);
    Task<int> MarkAllAsReadAsync();
    Task CreateAsync(string title, string message, string type, string? relatedEntityType, int? relatedEntityId);
}
