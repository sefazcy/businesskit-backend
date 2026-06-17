using System.ComponentModel.DataAnnotations;

namespace BusinessKit.Application.Staff.Dtos;

public class CreateStaffMemberDto
{
    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [RegularExpression("^[a-z0-9]+(-[a-z0-9]+)*$",
        ErrorMessage = "Slug must be lowercase, URL-safe, and use hyphens to separate words (e.g. 'jane-doe').")]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? Title { get; set; }

    [MaxLength(2000)]
    public string? Bio { get; set; }

    [MaxLength(500)]
    public string? PhotoUrl { get; set; }

    [EmailAddress]
    [MaxLength(200)]
    public string? Email { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(500)]
    public string? InstagramUrl { get; set; }

    [MaxLength(500)]
    public string? LinkedInUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public int DisplayOrder { get; set; }
}
