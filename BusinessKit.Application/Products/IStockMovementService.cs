using BusinessKit.Application.Products.Dtos;

namespace BusinessKit.Application.Products;

public interface IStockMovementService
{
    Task<List<StockMovementDto>> GetAllAsync(StockMovementListQuery query);
    Task<List<StockMovementDto>> GetByProductIdAsync(int productId);
    Task<StockMovementDto> CreateAsync(CreateStockMovementRequest request);
    Task<StockSummaryDto?> GetStockSummaryAsync(int productId);
}
