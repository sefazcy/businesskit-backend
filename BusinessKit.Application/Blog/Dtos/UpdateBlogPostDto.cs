using System.ComponentModel.DataAnnotations;

namespace BusinessKit.Application.Blog.Dtos;

public class UpdateBlogPostDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [RegularExpression("^[a-z0-9]+(-[a-z0-9]+)*$",
        ErrorMessage = "Slug must be lowercase, URL-safe, and use hyphens to separate words (e.g. 'web-design-for-small-businesses').")]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Summary { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? CoverImageUrl { get; set; }

    [MaxLength(200)]
    public string? SeoTitle { get; set; }

    [MaxLength(300)]
    public string? MetaDescription { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    [Required]
    [MaxLength(10)]
    public string Language { get; set; } = "en";

    public bool IsPublished { get; set; }

    public DateTime? PublishedAt { get; set; }
}
