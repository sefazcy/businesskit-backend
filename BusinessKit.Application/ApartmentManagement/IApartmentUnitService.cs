using BusinessKit.Application.ApartmentManagement.Dtos;

namespace BusinessKit.Application.ApartmentManagement;

public interface IApartmentUnitService
{
    Task<List<ApartmentUnitDto>> GetAllAsync(ApartmentUnitListQuery query);
    Task<ApartmentUnitDto?> GetByIdAsync(int id);
    Task<ApartmentUnitDto> CreateAsync(CreateApartmentUnitRequest request);
    Task<ApartmentUnitDto?> UpdateAsync(int id, UpdateApartmentUnitRequest request);
    Task<ApartmentUnitDto?> ToggleActiveAsync(int id);
}
