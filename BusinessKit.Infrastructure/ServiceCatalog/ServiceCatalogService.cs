using BusinessKit.Application.Exceptions;
using BusinessKit.Application.ServiceCatalog;
using BusinessKit.Application.ServiceCatalog.Dtos;
using BusinessKit.Domain.Entities;
using BusinessKit.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BusinessKit.Infrastructure.ServiceCatalog;

public class ServiceCatalogService : IServiceCatalogService
{
    private readonly AppDbContext _context;

    public ServiceCatalogService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ServiceDto>> GetActiveServicesAsync()
    {
        var services = await _context.BusinessServices
            .Where(s => s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Id)
            .ToListAsync();

        return services.Select(MapToDto).ToList();
    }

    public async Task<ServiceDto?> GetActiveServiceBySlugAsync(string slug)
    {
        var service = await _context.BusinessServices
            .FirstOrDefaultAsync(s => s.Slug == slug && s.IsActive);

        return service == null ? null : MapToDto(service);
    }

    public async Task<List<ServiceDto>> GetAllServicesAsync()
    {
        var services = await _context.BusinessServices
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.Id)
            .ToListAsync();

        return services.Select(MapToDto).ToList();
    }

    public async Task<ServiceDto?> GetServiceByIdAsync(int id)
    {
        var service = await _context.BusinessServices.FindAsync(id);
        return service == null ? null : MapToDto(service);
    }

    public async Task<ServiceDto> CreateServiceAsync(CreateServiceDto dto)
    {
        await EnsureSlugIsUniqueAsync(dto.Slug, excludeId: null);

        var service = new BusinessService
        {
            Title = dto.Title,
            Slug = dto.Slug,
            ShortDescription = dto.ShortDescription,
            FullDescription = dto.FullDescription,
            Price = dto.Price,
            DurationMinutes = dto.DurationMinutes,
            ImageUrl = dto.ImageUrl,
            IsActive = true,
            DisplayOrder = dto.DisplayOrder
        };

        _context.BusinessServices.Add(service);
        await _context.SaveChangesAsync();

        return MapToDto(service);
    }

    public async Task<ServiceDto?> UpdateServiceAsync(int id, UpdateServiceDto dto)
    {
        var service = await _context.BusinessServices.FindAsync(id);
        if (service == null)
            return null;

        await EnsureSlugIsUniqueAsync(dto.Slug, excludeId: id);

        service.Title = dto.Title;
        service.Slug = dto.Slug;
        service.ShortDescription = dto.ShortDescription;
        service.FullDescription = dto.FullDescription;
        service.Price = dto.Price;
        service.DurationMinutes = dto.DurationMinutes;
        service.ImageUrl = dto.ImageUrl;
        service.IsActive = dto.IsActive;
        service.DisplayOrder = dto.DisplayOrder;

        await _context.SaveChangesAsync();

        return MapToDto(service);
    }

    public async Task<ServiceDto?> ToggleActiveAsync(int id)
    {
        var service = await _context.BusinessServices.FindAsync(id);
        if (service == null)
            return null;

        service.IsActive = !service.IsActive;
        await _context.SaveChangesAsync();

        return MapToDto(service);
    }

    private async Task EnsureSlugIsUniqueAsync(string slug, int? excludeId)
    {
        var normalizedSlug = slug.ToLowerInvariant();

        var isTaken = await _context.BusinessServices
            .AnyAsync(s => s.Slug.ToLower() == normalizedSlug && s.Id != excludeId);

        if (isTaken)
            throw new DuplicateSlugException(slug);
    }

    private static ServiceDto MapToDto(BusinessService s) => new()
    {
        Id = s.Id,
        Title = s.Title,
        Slug = s.Slug,
        ShortDescription = s.ShortDescription,
        FullDescription = s.FullDescription,
        Price = s.Price,
        DurationMinutes = s.DurationMinutes,
        ImageUrl = s.ImageUrl,
        IsActive = s.IsActive,
        DisplayOrder = s.DisplayOrder,
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt
    };
}
