using System.ComponentModel.DataAnnotations;

namespace BusinessKit.Application.Gallery.Dtos;

public class CreateGalleryItemDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Category { get; set; }

    public int DisplayOrder { get; set; }
}
