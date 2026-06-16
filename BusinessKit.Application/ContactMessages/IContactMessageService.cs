using BusinessKit.Application.ContactMessages.Dtos;

namespace BusinessKit.Application.ContactMessages;

public interface IContactMessageService
{
    Task<ContactMessageSubmittedDto> CreateAsync(CreateContactMessageDto dto, string? ipAddress);

    Task<List<ContactMessageDto>> GetAllAsync(bool? unreadOnly, bool? archivedOnly);
    Task<ContactMessageDto?> GetByIdAsync(int id);

    Task<ContactMessageDto?> MarkReadAsync(int id);
    Task<ContactMessageDto?> MarkUnreadAsync(int id);
    Task<ContactMessageDto?> MarkRepliedAsync(int id);
    Task<ContactMessageDto?> ArchiveAsync(int id);
    Task<ContactMessageDto?> UnarchiveAsync(int id);
}
