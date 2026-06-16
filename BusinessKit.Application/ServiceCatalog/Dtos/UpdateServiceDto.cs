using System.ComponentModel.DataAnnotations;

namespace BusinessKit.Application.ServiceCatalog.Dtos;

public class UpdateServiceDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [RegularExpression("^[a-z0-9]+(-[a-z0-9]+)*$",
        ErrorMessage = "Slug must be lowercase, URL-safe, and use hyphens to separate words (e.g. 'haircut-and-beard').")]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ShortDescription { get; set; }

    public string? FullDescription { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int DurationMinutes { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; }

    public int DisplayOrder { get; set; }
}
