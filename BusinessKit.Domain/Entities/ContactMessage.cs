namespace BusinessKit.Domain.Entities;

public class ContactMessage : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Subject { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public bool IsReplied { get; set; }
    public bool IsArchived { get; set; }
    public string? IpAddress { get; set; }
}
