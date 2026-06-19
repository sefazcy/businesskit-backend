using BusinessKit.Application.ContactMessages;
using BusinessKit.Application.ContactMessages.Dtos;
using BusinessKit.Application.Email;
using BusinessKit.Domain.Entities;
using BusinessKit.Infrastructure.Data;
using BusinessKit.Infrastructure.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BusinessKit.Infrastructure.ContactMessages;

public class ContactMessageService : IContactMessageService
{
    private readonly AppDbContext _context;
    private readonly IEmailSender _emailSender;
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<ContactMessageService> _logger;

    public ContactMessageService(
        AppDbContext context,
        IEmailSender emailSender,
        IOptions<EmailSettings> emailOptions,
        ILogger<ContactMessageService> logger)
    {
        _context = context;
        _emailSender = emailSender;
        _emailSettings = emailOptions.Value;
        _logger = logger;
    }

    public async Task<ContactMessageSubmittedDto> CreateAsync(CreateContactMessageDto dto, string? ipAddress)
    {
        var message = new ContactMessage
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone,
            Subject = dto.Subject,
            Message = dto.Message,
            IsRead = false,
            IsReplied = false,
            IsArchived = false,
            IpAddress = ipAddress
        };

        _context.ContactMessages.Add(message);
        await _context.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(_emailSettings.AdminEmail))
        {
            try
            {
                var businessName = string.IsNullOrWhiteSpace(_emailSettings.FromName) ? "BusinessKit" : _emailSettings.FromName;

                var (subject, html) = EmailTemplates.ContactMessageAdmin(
                    dto.FullName,
                    dto.Email,
                    dto.Phone,
                    dto.Subject,
                    dto.Message,
                    businessName);

                await _emailSender.SendAsync(_emailSettings.AdminEmail, "Admin", subject, html);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send contact-message admin notification for message #{Id}.", message.Id);
            }
        }

        return new ContactMessageSubmittedDto
        {
            Id = message.Id,
            CreatedAt = message.CreatedAt
        };
    }

    public async Task<List<ContactMessageDto>> GetAllAsync(bool? unreadOnly, bool? archivedOnly)
    {
        var query = _context.ContactMessages.AsQueryable();

        if (unreadOnly == true)
            query = query.Where(m => !m.IsRead);

        if (archivedOnly == true)
            query = query.Where(m => m.IsArchived);
        else if (archivedOnly == false)
            query = query.Where(m => !m.IsArchived);

        var messages = await query
            .OrderByDescending(m => m.CreatedAt)
            .ThenByDescending(m => m.Id)
            .ToListAsync();

        return messages.Select(MapToDto).ToList();
    }

    public async Task<ContactMessageDto?> GetByIdAsync(int id)
    {
        var message = await _context.ContactMessages.FindAsync(id);
        return message == null ? null : MapToDto(message);
    }

    public async Task<ContactMessageDto?> MarkReadAsync(int id) =>
        await UpdateStatusAsync(id, m => m.IsRead = true);

    public async Task<ContactMessageDto?> MarkUnreadAsync(int id) =>
        await UpdateStatusAsync(id, m => m.IsRead = false);

    public async Task<ContactMessageDto?> MarkRepliedAsync(int id) =>
        await UpdateStatusAsync(id, m => m.IsReplied = true);

    public async Task<ContactMessageDto?> ArchiveAsync(int id) =>
        await UpdateStatusAsync(id, m => m.IsArchived = true);

    public async Task<ContactMessageDto?> UnarchiveAsync(int id) =>
        await UpdateStatusAsync(id, m => m.IsArchived = false);

    private async Task<ContactMessageDto?> UpdateStatusAsync(int id, Action<ContactMessage> applyChange)
    {
        var message = await _context.ContactMessages.FindAsync(id);
        if (message == null)
            return null;

        applyChange(message);
        await _context.SaveChangesAsync();

        return MapToDto(message);
    }

    private static ContactMessageDto MapToDto(ContactMessage m) => new()
    {
        Id = m.Id,
        FullName = m.FullName,
        Email = m.Email,
        Phone = m.Phone,
        Subject = m.Subject,
        Message = m.Message,
        IsRead = m.IsRead,
        IsReplied = m.IsReplied,
        IsArchived = m.IsArchived,
        IpAddress = m.IpAddress,
        CreatedAt = m.CreatedAt,
        UpdatedAt = m.UpdatedAt
    };
}
