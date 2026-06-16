using BusinessKit.Application.Gallery.Dtos;

namespace BusinessKit.Application.Gallery;

public interface IGalleryService
{
    Task<List<GalleryItemDto>> GetActiveItemsAsync(string? category);
    Task<GalleryItemDto?> GetActiveItemByIdAsync(int id);

    Task<List<GalleryItemDto>> GetAllItemsAsync(string? category, bool? isActive);
    Task<GalleryItemDto?> GetItemByIdAsync(int id);
    Task<GalleryItemDto> CreateItemAsync(CreateGalleryItemDto dto);
    Task<GalleryItemDto?> UpdateItemAsync(int id, UpdateGalleryItemDto dto);
    Task<GalleryItemDto?> ToggleActiveAsync(int id);
}
