namespace BusinessKit.Application.ContactMessages.Dtos;

public class ContactMessageDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Subject { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public bool IsReplied { get; set; }
    public bool IsArchived { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
