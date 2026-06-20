using BusinessKit.Application.BusinessSettings;
using BusinessKit.Application.BusinessSettings.Dtos;
using BusinessKit.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using BusinessSettingsEntity = BusinessKit.Domain.Entities.BusinessSettings;

namespace BusinessKit.Infrastructure.BusinessSettings;

public class BusinessSettingsService : IBusinessSettingsService
{
    private readonly AppDbContext _context;

    public BusinessSettingsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<BusinessSettingsDto?> GetAsync()
    {
        var settings = await _context.BusinessSettings.FirstOrDefaultAsync();
        return settings == null ? null : MapToDto(settings);
    }

    public async Task<BusinessSettingsDto> UpsertAsync(UpdateBusinessSettingsDto dto)
    {
        var settings = await _context.BusinessSettings.FirstOrDefaultAsync();

        if (settings == null)
        {
            settings = new BusinessSettingsEntity();
            _context.BusinessSettings.Add(settings);
        }

        settings.BusinessName = dto.BusinessName;
        settings.LogoUrl = dto.LogoUrl;
        settings.Phone = dto.Phone;
        settings.Email = dto.Email;
        settings.Address = dto.Address;
        settings.WhatsApp = dto.WhatsApp;
        settings.InstagramUrl = dto.Instagram;
        settings.LinkedInUrl = dto.LinkedIn;
        settings.FacebookUrl = dto.Facebook;
        settings.TwitterUrl = dto.Twitter;
        settings.WebsiteUrl = dto.Website;
        settings.WorkingHours = dto.WorkingHours;
        settings.Currency = dto.Currency.Trim().ToUpperInvariant();
        settings.ThemeColor = dto.ThemeColor;

        await _context.SaveChangesAsync();

        return MapToDto(settings);
    }

    private static BusinessSettingsDto MapToDto(BusinessSettingsEntity s) => new()
    {
        BusinessName = s.BusinessName,
        LogoUrl = s.LogoUrl,
        Phone = s.Phone,
        Email = s.Email,
        Address = s.Address,
        WhatsApp = s.WhatsApp,
        Instagram = s.InstagramUrl,
        LinkedIn = s.LinkedInUrl,
        Facebook = s.FacebookUrl,
        Twitter = s.TwitterUrl,
        Website = s.WebsiteUrl,
        WorkingHours = s.WorkingHours,
        Currency = s.Currency,
        ThemeColor = s.ThemeColor,
        UpdatedAt = s.UpdatedAt
    };
}
