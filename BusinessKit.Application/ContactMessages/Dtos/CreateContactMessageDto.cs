using System.ComponentModel.DataAnnotations;

namespace BusinessKit.Application.ContactMessages.Dtos;

public class CreateContactMessageDto
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(200)]
    public string? Subject { get; set; }

    [Required]
    [MaxLength(2000)]
    public string Message { get; set; } = string.Empty;
}
