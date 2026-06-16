using BusinessKit.Application.Blog;
using BusinessKit.Application.Blog.Dtos;
using BusinessKit.Application.Exceptions;
using BusinessKit.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusinessKit.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/blog")]
[Authorize(Roles = Roles.Admin)]
[Tags("Blog (Admin)")]
public class AdminBlogController : ControllerBase
{
    private readonly IBlogService _blogService;

    public AdminBlogController(IBlogService blogService)
    {
        _blogService = blogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? language,
        [FromQuery] string? category,
        [FromQuery] bool? isPublished)
    {
        var posts = await _blogService.GetAllPostsAsync(language, category, isPublished);
        return Ok(posts);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var post = await _blogService.GetPostByIdAsync(id);
        if (post == null)
            return NotFound(new { message = $"Blog post with id {id} was not found." });

        return Ok(post);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBlogPostDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var post = await _blogService.CreatePostAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = post.Id }, post);
        }
        catch (DuplicateBlogSlugException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBlogPostDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var post = await _blogService.UpdatePostAsync(id, dto);
            if (post == null)
                return NotFound(new { message = $"Blog post with id {id} was not found." });

            return Ok(post);
        }
        catch (DuplicateBlogSlugException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:int}/publish")]
    public async Task<IActionResult> Publish(int id)
    {
        var post = await _blogService.PublishAsync(id);
        if (post == null)
            return NotFound(new { message = $"Blog post with id {id} was not found." });

        return Ok(post);
    }

    [HttpPatch("{id:int}/unpublish")]
    public async Task<IActionResult> Unpublish(int id)
    {
        var post = await _blogService.UnpublishAsync(id);
        if (post == null)
            return NotFound(new { message = $"Blog post with id {id} was not found." });

        return Ok(post);
    }
}
