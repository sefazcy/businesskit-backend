using BusinessKit.Application.Notifications;
using BusinessKit.Application.Notifications.Dtos;
using BusinessKit.Domain.Entities;
using BusinessKit.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BusinessKit.Infrastructure.Notifications;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _context;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(AppDbContext context, ILogger<NotificationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<NotificationDto>> GetAllAsync(bool? unreadOnly = null, string? type = null, int take = 50)
    {
        if (take <= 0) take = 50;
        if (take > 200) take = 200;

        var query = _context.Notifications.AsQueryable();

        if (unreadOnly == true)
            query = query.Where(n => !n.IsRead);

        var typeTrim = type?.Trim();
        if (!string.IsNullOrWhiteSpace(typeTrim))
            query = query.Where(n => n.Type == typeTrim);

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .ThenByDescending(n => n.Id)
            .Take(take)
            .ToListAsync();

        return notifications.Select(MapToDto).ToList();
    }

    public async Task<int> GetUnreadCountAsync()
    {
        return await _context.Notifications.CountAsync(n => !n.IsRead);
    }

    public async Task<NotificationSummaryDto> GetSummaryAsync()
    {
        return new NotificationSummaryDto
        {
            UnreadCount = await GetUnreadCountAsync()
        };
    }

    public async Task<NotificationDto?> MarkAsReadAsync(int id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification == null)
            return null;

        notification.IsRead = true;
        notification.ReadAt ??= DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(notification);
    }

    public async Task<NotificationDto?> MarkAsUnreadAsync(int id)
    {
        var notification = await _context.Notifications.FindAsync(id);
        if (notification == null)
            return null;

        notification.IsRead = false;
        notification.ReadAt = null;

        await _context.SaveChangesAsync();

        return MapToDto(notification);
    }

    public async Task<int> MarkAllAsReadAsync()
    {
        var now = DateTime.UtcNow;

        return await _context.Notifications
            .Where(n => !n.IsRead)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.ReadAt, now));
    }

    public async Task CreateAsync(
        string title,
        string message,
        string type,
        string? relatedEntityType,
        int? relatedEntityId)
    {
        try
        {
            var notification = new Notification
            {
                Title = title.Trim(),
                Message = message.Trim(),
                Type = type.Trim(),
                RelatedEntityType = relatedEntityType?.Trim(),
                RelatedEntityId = relatedEntityId,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create notification of type {Type}.", type);
        }
    }

    private static NotificationDto MapToDto(Notification n) => new()
    {
        Id = n.Id,
        Title = n.Title,
        Message = n.Message,
        Type = n.Type,
        RelatedEntityType = n.RelatedEntityType,
        RelatedEntityId = n.RelatedEntityId,
        IsRead = n.IsRead,
        ReadAt = n.ReadAt,
        CreatedAt = n.CreatedAt
    };
}
