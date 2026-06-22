using BusinessKit.Application.Exceptions;
using BusinessKit.Application.Products;
using BusinessKit.Application.Products.Dtos;
using BusinessKit.Domain.Entities;
using BusinessKit.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BusinessKit.Infrastructure.Products;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductDto>> GetAllAsync(ProductListQuery query)
    {
        var q = _context.Products.AsQueryable();

        if (query.IsActive.HasValue)
            q = q.Where(p => p.IsActive == query.IsActive.Value);

        var searchTerm = query.Search?.Trim();
        if (!string.IsNullOrWhiteSpace(searchTerm))
            q = q.Where(p =>
                EF.Functions.Like(p.Name, $"%{searchTerm}%") ||
                EF.Functions.Like(p.Sku ?? "", $"%{searchTerm}%") ||
                EF.Functions.Like(p.Category ?? "", $"%{searchTerm}%"));

        var categoryTerm = query.Category?.Trim();
        if (!string.IsNullOrWhiteSpace(categoryTerm))
            q = q.Where(p => EF.Functions.Like(p.Category ?? "", $"%{categoryTerm}%"));

        if (query.LowStockOnly)
            q = q.Where(p => p.MinStock > 0 && p.CurrentStock <= p.MinStock);

        var products = await q
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Id)
            .Take(query.Take > 0 ? query.Take : 50)
            .ToListAsync();

        return products.Select(MapToDto).ToList();
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        return product == null ? null : MapToDto(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request)
    {
        await EnsureSkuUniqueAsync(request.Sku, excludeId: null);

        var product = new Product
        {
            Name         = request.Name.Trim(),
            Sku          = string.IsNullOrWhiteSpace(request.Sku) ? null : request.Sku.Trim(),
            Category     = string.IsNullOrWhiteSpace(request.Category) ? null : request.Category.Trim(),
            Unit         = request.Unit.Trim(),
            CurrentStock = request.CurrentStock,
            MinStock     = request.MinStock,
            CostPrice    = request.CostPrice,
            SalePrice    = request.SalePrice,
            IsActive     = request.IsActive,
            Notes        = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return MapToDto(product);
    }

    public async Task<ProductDto?> UpdateAsync(int id, UpdateProductRequest request)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return null;

        await EnsureSkuUniqueAsync(request.Sku, excludeId: id);

        product.Name         = request.Name.Trim();
        product.Sku          = string.IsNullOrWhiteSpace(request.Sku) ? null : request.Sku.Trim();
        product.Category     = string.IsNullOrWhiteSpace(request.Category) ? null : request.Category.Trim();
        product.Unit         = request.Unit.Trim();
        product.CurrentStock = request.CurrentStock;
        product.MinStock     = request.MinStock;
        product.CostPrice    = request.CostPrice;
        product.SalePrice    = request.SalePrice;
        product.IsActive     = request.IsActive;
        product.Notes        = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

        await _context.SaveChangesAsync();

        return MapToDto(product);
    }

    public async Task<ProductDto?> ToggleActiveAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return null;

        product.IsActive = !product.IsActive;
        await _context.SaveChangesAsync();

        return MapToDto(product);
    }

    public async Task<List<string>> GetCategoriesAsync()
    {
        return await _context.Products
            .Where(p => p.Category != null)
            .Select(p => p.Category!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    private async Task EnsureSkuUniqueAsync(string? sku, int? excludeId)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return;

        var trimmed = sku.Trim();
        var taken = await _context.Products.AnyAsync(p =>
            p.Sku == trimmed && (excludeId == null || p.Id != excludeId));

        if (taken)
            throw new DuplicateSkuException(trimmed);
    }

    private static ProductDto MapToDto(Product p) => new()
    {
        Id           = p.Id,
        Name         = p.Name,
        Sku          = p.Sku,
        Category     = p.Category,
        Unit         = p.Unit,
        CurrentStock = p.CurrentStock,
        MinStock     = p.MinStock,
        CostPrice    = p.CostPrice,
        SalePrice    = p.SalePrice,
        IsActive     = p.IsActive,
        IsLowStock   = p.MinStock > 0 && p.CurrentStock <= p.MinStock,
        Notes        = p.Notes,
        CreatedAt    = p.CreatedAt,
        UpdatedAt    = p.UpdatedAt,
    };
}
