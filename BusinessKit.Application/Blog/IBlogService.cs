using BusinessKit.Application.Blog.Dtos;

namespace BusinessKit.Application.Blog;

public interface IBlogService
{
    Task<List<BlogPostDto>> GetPublishedPostsAsync(string? language, string? category);
    Task<BlogPostDto?> GetPublishedPostBySlugAsync(string slug, string? language);

    Task<List<BlogPostDto>> GetAllPostsAsync(string? language, string? category, bool? isPublished);
    Task<BlogPostDto?> GetPostByIdAsync(int id);
    Task<BlogPostDto> CreatePostAsync(CreateBlogPostDto dto);
    Task<BlogPostDto?> UpdatePostAsync(int id, UpdateBlogPostDto dto);
    Task<BlogPostDto?> PublishAsync(int id);
    Task<BlogPostDto?> UnpublishAsync(int id);
}
