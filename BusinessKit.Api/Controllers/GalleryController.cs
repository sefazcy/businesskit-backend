using BusinessKit.Application.Gallery;
using Microsoft.AspNetCore.Mvc;

namespace BusinessKit.Api.Controllers;

[ApiController]
[Route("api/gallery")]
public class GalleryController : ControllerBase
{
    private readonly IGalleryService _galleryService;

    public GalleryController(IGalleryService galleryService)
    {
        _galleryService = galleryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetActive([FromQuery] string? category)
    {
        var items = await _galleryService.GetActiveItemsAsync(category);
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _galleryService.GetActiveItemByIdAsync(id);
        if (item == null)
            return NotFound(new { message = $"Gallery item with id {id} was not found." });

        return Ok(item);
    }
}
