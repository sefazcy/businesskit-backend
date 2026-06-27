using BusinessKit.Application.ApartmentManagement.Dtos;

namespace BusinessKit.Application.ApartmentManagement;

public interface IResidentService
{
    Task<List<ResidentDto>> GetAllAsync(ResidentListQuery query);
    Task<ResidentDto?> GetByIdAsync(int id);
    Task<List<ResidentDto>> GetByUnitIdAsync(int apartmentUnitId);
    Task<ResidentDto> CreateAsync(CreateResidentRequest request);
    Task<ResidentDto?> UpdateAsync(int id, UpdateResidentRequest request);
    Task<ResidentDto?> ToggleActiveAsync(int id);
}
