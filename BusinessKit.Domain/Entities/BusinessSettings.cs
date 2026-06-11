namespace BusinessKit.Domain.Entities;

public class BusinessSettings : BaseEntity
{
    public string BusinessName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? WhatsApp { get; set; }
    public string? InstagramUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public string? TwitterUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? WorkingHours { get; set; }
    public string Currency { get; set; } = "USD";
    public string? ThemeColor { get; set; }
}