using BusinessKit.Application.BusinessSettings.Dtos;

namespace BusinessKit.Application.BusinessSettings;

public interface IBusinessSettingsService
{
    Task<BusinessSettingsDto?> GetAsync();
    Task<BusinessSettingsDto> UpsertAsync(UpdateBusinessSettingsDto dto);
}
