using BusinessKit.Application.Gallery;
using BusinessKit.Application.Gallery.Dtos;
using BusinessKit.Domain.Entities;
using BusinessKit.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BusinessKit.Infrastructure.Gallery;

public class GalleryService : IGalleryService
{
    private readonly AppDbContext _context;

    public GalleryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<GalleryItemDto>> GetActiveItemsAsync(string? category)
    {
        var query = _context.GalleryItems.Where(g => g.IsActive);
        query = ApplyCategoryFilter(query, category);

        var items = await query
            .OrderBy(g => g.DisplayOrder)
            .ThenBy(g => g.Id)
            .ToListAsync();

        return items.Select(MapToDto).ToList();
    }

    public async Task<GalleryItemDto?> GetActiveItemByIdAsync(int id)
    {
        var item = await _context.GalleryItems
            .FirstOrDefaultAsync(g => g.Id == id && g.IsActive);

        return item == null ? null : MapToDto(item);
    }

    public async Task<List<GalleryItemDto>> GetAllItemsAsync(string? category, bool? isActive)
    {
        var query = _context.GalleryItems.AsQueryable();
        query = ApplyCategoryFilter(query, category);

        if (isActive.HasValue)
            query = query.Where(g => g.IsActive == isActive.Value);

        var items = await query
            .OrderBy(g => g.DisplayOrder)
            .ThenBy(g => g.Id)
            .ToListAsync();

        return items.Select(MapToDto).ToList();
    }

    public async Task<GalleryItemDto?> GetItemByIdAsync(int id)
    {
        var item = await _context.GalleryItems.FindAsync(id);
        return item == null ? null : MapToDto(item);
    }

    public async Task<GalleryItemDto> CreateItemAsync(CreateGalleryItemDto dto)
    {
        var item = new GalleryItem
        {
            Title = dto.Title,
            Description = dto.Description,
            ImageUrl = dto.ImageUrl,
            Category = dto.Category,
            IsActive = true,
            DisplayOrder = dto.DisplayOrder
        };

        _context.GalleryItems.Add(item);
        await _context.SaveChangesAsync();

        return MapToDto(item);
    }

    public async Task<GalleryItemDto?> UpdateItemAsync(int id, UpdateGalleryItemDto dto)
    {
        var item = await _context.GalleryItems.FindAsync(id);
        if (item == null)
            return null;

        item.Title = dto.Title;
        item.Description = dto.Description;
        item.ImageUrl = dto.ImageUrl;
        item.Category = dto.Category;
        item.IsActive = dto.IsActive;
        item.DisplayOrder = dto.DisplayOrder;

        await _context.SaveChangesAsync();

        return MapToDto(item);
    }

    public async Task<GalleryItemDto?> ToggleActiveAsync(int id)
    {
        var item = await _context.GalleryItems.FindAsync(id);
        if (item == null)
            return null;

        item.IsActive = !item.IsActive;
        await _context.SaveChangesAsync();

        return MapToDto(item);
    }

    private static IQueryable<GalleryItem> ApplyCategoryFilter(IQueryable<GalleryItem> query, string? category)
    {
        if (string.IsNullOrWhiteSpace(category))
            return query;

        var normalizedCategory = category.ToLower();
        return query.Where(g => g.Category != null && g.Category.ToLower() == normalizedCategory);
    }

    private static GalleryItemDto MapToDto(GalleryItem g) => new()
    {
        Id = g.Id,
        Title = g.Title,
        Description = g.Description,
        ImageUrl = g.ImageUrl,
        Category = g.Category,
        IsActive = g.IsActive,
        DisplayOrder = g.DisplayOrder,
        CreatedAt = g.CreatedAt,
        UpdatedAt = g.UpdatedAt
    };
}
