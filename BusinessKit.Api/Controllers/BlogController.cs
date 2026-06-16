using BusinessKit.Application.Blog;
using Microsoft.AspNetCore.Mvc;

namespace BusinessKit.Api.Controllers;

[ApiController]
[Route("api/blog")]
public class BlogController : ControllerBase
{
    private readonly IBlogService _blogService;

    public BlogController(IBlogService blogService)
    {
        _blogService = blogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPublished([FromQuery] string? language, [FromQuery] string? category)
    {
        var posts = await _blogService.GetPublishedPostsAsync(language, category);
        return Ok(posts);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, [FromQuery] string? language)
    {
        var post = await _blogService.GetPublishedPostBySlugAsync(slug, language);
        if (post == null)
            return NotFound(new { message = $"Blog post with slug '{slug}' was not found." });

        return Ok(post);
    }
}
