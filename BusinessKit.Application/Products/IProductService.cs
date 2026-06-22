using BusinessKit.Application.Products.Dtos;

namespace BusinessKit.Application.Products;

public interface IProductService
{
    Task<List<ProductDto>> GetAllAsync(ProductListQuery query);
    Task<ProductDto?> GetByIdAsync(int id);
    Task<ProductDto> CreateAsync(CreateProductRequest request);
    Task<ProductDto?> UpdateAsync(int id, UpdateProductRequest request);
    Task<ProductDto?> ToggleActiveAsync(int id);
    Task<List<string>> GetCategoriesAsync();
}
