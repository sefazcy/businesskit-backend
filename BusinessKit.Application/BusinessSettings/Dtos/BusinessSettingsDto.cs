namespace BusinessKit.Application.BusinessSettings.Dtos;

public class BusinessSettingsDto
{
    public string BusinessName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? WhatsApp { get; set; }
    public string? Instagram { get; set; }
    public string? LinkedIn { get; set; }
    public string? Facebook { get; set; }
    public string? Twitter { get; set; }
    public string? Website { get; set; }
    public string? WorkingHours { get; set; }
    public string Currency { get; set; } = "USD";
    public string? ThemeColor { get; set; }
    public DateTime UpdatedAt { get; set; }
}
