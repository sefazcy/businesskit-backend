using BusinessKit.Application.Exceptions;
using BusinessKit.Application.Products;
using BusinessKit.Application.Products.Dtos;
using BusinessKit.Domain.Entities;
using BusinessKit.Infrastructure.Data;
using BusinessKit.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace BusinessKit.Infrastructure.Products;

public class StockMovementService : IStockMovementService
{
    private readonly AppDbContext _context;

    public StockMovementService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<StockMovementDto>> GetAllAsync(StockMovementListQuery query)
    {
        var q = _context.StockMovements
            .Include(sm => sm.Product)
            .AsQueryable();

        if (query.ProductId.HasValue)
            q = q.Where(sm => sm.ProductId == query.ProductId.Value);

        if (!string.IsNullOrWhiteSpace(query.Type))
            q = q.Where(sm => sm.Type == query.Type.Trim());

        if (query.DateFrom.HasValue)
            q = q.Where(sm => sm.CreatedAt >= query.DateFrom.Value);

        if (query.DateTo.HasValue)
            q = q.Where(sm => sm.CreatedAt <= query.DateTo.Value);

        var movements = await q
            .OrderByDescending(sm => sm.CreatedAt)
            .ThenByDescending(sm => sm.Id)
            .Take(query.Take > 0 ? query.Take : 100)
            .ToListAsync();

        return movements.Select(MapToDto).ToList();
    }

    public async Task<List<StockMovementDto>> GetByProductIdAsync(int productId)
    {
        var movements = await _context.StockMovements
            .Include(sm => sm.Product)
            .Where(sm => sm.ProductId == productId)
            .OrderByDescending(sm => sm.CreatedAt)
            .ThenByDescending(sm => sm.Id)
            .Take(500)
            .ToListAsync();

        return movements.Select(MapToDto).ToList();
    }

    public async Task<StockMovementDto> CreateAsync(CreateStockMovementRequest request)
    {
        if (!StockMovementTypes.IsValid(request.Type))
            throw new InvalidStockMovementTypeException(request.Type);

        var product = await _context.Products.FindAsync(request.ProductId);
        if (product == null)
            throw new KeyNotFoundException($"Product with id {request.ProductId} was not found.");

        var previousStock = product.CurrentStock;
        decimal newStock;

        switch (request.Type)
        {
            case StockMovementTypes.In:
                if (request.Quantity <= 0)
                    throw new ArgumentException("Quantity must be greater than 0 for In movements.");
                newStock = previousStock + request.Quantity;
                break;

            case StockMovementTypes.Out:
                if (request.Quantity <= 0)
                    throw new ArgumentException("Quantity must be greater than 0 for Out movements.");
                newStock = previousStock - request.Quantity;
                if (newStock < 0)
                    throw new InsufficientStockException(product.Name, previousStock, request.Quantity);
                break;

            case StockMovementTypes.Adjustment:
                // Quantity represents the desired final stock value.
                if (request.Quantity < 0)
                    throw new ArgumentException("Quantity cannot be negative for Adjustment movements.");
                newStock = request.Quantity;
                break;

            default:
                throw new InvalidStockMovementTypeException(request.Type);
        }

        var movement = new StockMovement
        {
            ProductId     = request.ProductId,
            Type          = request.Type,
            Quantity      = request.Quantity,
            PreviousStock = previousStock,
            NewStock      = newStock,
            Reason        = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim(),
            Notes         = string.IsNullOrWhiteSpace(request.Notes)  ? null : request.Notes.Trim(),
        };

        product.CurrentStock = newStock;
        // Product.UpdatedAt is stamped automatically by AppDbContext.StampEntities.

        _context.StockMovements.Add(movement);
        await _context.SaveChangesAsync();

        // Attach the product reference for MapToDto after save.
        movement.Product = product;
        return MapToDto(movement);
    }

    public async Task<StockSummaryDto?> GetStockSummaryAsync(int productId)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null)
            return null;

        var baseQuery = _context.StockMovements.Where(sm => sm.ProductId == productId);

        var totalIn = await baseQuery
            .Where(sm => sm.Type == StockMovementTypes.In)
            .SumAsync(sm => (decimal?)sm.Quantity) ?? 0m;

        var totalOut = await baseQuery
            .Where(sm => sm.Type == StockMovementTypes.Out)
            .SumAsync(sm => (decimal?)sm.Quantity) ?? 0m;

        var adjustmentCount = await baseQuery
            .CountAsync(sm => sm.Type == StockMovementTypes.Adjustment);

        var lastMovementAt = await baseQuery
            .OrderByDescending(sm => sm.CreatedAt)
            .Select(sm => (DateTime?)sm.CreatedAt)
            .FirstOrDefaultAsync();

        return new StockSummaryDto
        {
            ProductId       = product.Id,
            ProductName     = product.Name,
            CurrentStock    = product.CurrentStock,
            MinStock        = product.MinStock,
            IsLowStock      = product.MinStock > 0 && product.CurrentStock <= product.MinStock,
            TotalIn         = totalIn,
            TotalOut        = totalOut,
            AdjustmentCount = adjustmentCount,
            LastMovementAt  = lastMovementAt,
        };
    }

    private static StockMovementDto MapToDto(StockMovement sm) => new()
    {
        Id            = sm.Id,
        ProductId     = sm.ProductId,
        ProductName   = sm.Product.Name,
        ProductSku    = sm.Product.Sku,
        Type          = sm.Type,
        Quantity      = sm.Quantity,
        PreviousStock = sm.PreviousStock,
        NewStock      = sm.NewStock,
        Reason        = sm.Reason,
        Notes         = sm.Notes,
        CreatedAt     = sm.CreatedAt,
    };
}
