using System.ComponentModel.DataAnnotations;
using BusinessKit.Application.BusinessSettings.Validation;

namespace BusinessKit.Application.BusinessSettings.Dtos;

public class UpdateBusinessSettingsDto
{
    [Required]
    [MaxLength(200)]
    public string BusinessName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? LogoUrl { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    [EmailAddress]
    [MaxLength(200)]
    public string? Email { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(30)]
    public string? WhatsApp { get; set; }

    [MaxLength(500)]
    public string? Instagram { get; set; }

    [MaxLength(500)]
    public string? LinkedIn { get; set; }

    [MaxLength(500)]
    public string? Facebook { get; set; }

    [MaxLength(500)]
    public string? Twitter { get; set; }

    [MaxLength(500)]
    public string? Website { get; set; }

    public string? WorkingHours { get; set; }

    [Required]
    [MaxLength(10)]
    [AllowedCurrency]
    public string Currency { get; set; } = "TRY";

    [MaxLength(20)]
    public string? ThemeColor { get; set; }
}
