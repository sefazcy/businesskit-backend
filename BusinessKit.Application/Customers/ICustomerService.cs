using BusinessKit.Application.Customers.Dtos;

namespace BusinessKit.Application.Customers;

public interface ICustomerService
{
    Task<List<CustomerDto>> GetAllAsync(string? name = null, string? email = null, string? phone = null, bool includeArchived = false);
    Task<CustomerDto?> GetByIdAsync(int id);
    Task<CustomerDto> CreateAsync(CreateCustomerDto dto);
    Task<CustomerDto?> UpdateAsync(int id, UpdateCustomerDto dto);
    Task<CustomerDto?> ArchiveAsync(int id);
    Task<CustomerDto?> UnarchiveAsync(int id);
}
