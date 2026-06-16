using BusinessKit.Application.ServiceCatalog.Dtos;

namespace BusinessKit.Application.ServiceCatalog;

public interface IServiceCatalogService
{
    Task<List<ServiceDto>> GetActiveServicesAsync();
    Task<ServiceDto?> GetActiveServiceBySlugAsync(string slug);

    Task<List<ServiceDto>> GetAllServicesAsync();
    Task<ServiceDto?> GetServiceByIdAsync(int id);
    Task<ServiceDto> CreateServiceAsync(CreateServiceDto dto);
    Task<ServiceDto?> UpdateServiceAsync(int id, UpdateServiceDto dto);
    Task<ServiceDto?> ToggleActiveAsync(int id);
}
