using BusinessKit.Application.Gallery;
using BusinessKit.Application.Gallery.Dtos;
using BusinessKit.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusinessKit.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/gallery")]
[Authorize(Roles = Roles.Admin)]
public class AdminGalleryController : ControllerBase
{
    private readonly IGalleryService _galleryService;

    public AdminGalleryController(IGalleryService galleryService)
    {
        _galleryService = galleryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? category, [FromQuery] bool? isActive)
    {
        var items = await _galleryService.GetAllItemsAsync(category, isActive);
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _galleryService.GetItemByIdAsync(id);
        if (item == null)
            return NotFound(new { message = $"Gallery item with id {id} was not found." });

        return Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGalleryItemDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var item = await _galleryService.CreateItemAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateGalleryItemDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var item = await _galleryService.UpdateItemAsync(id, dto);
        if (item == null)
            return NotFound(new { message = $"Gallery item with id {id} was not found." });

        return Ok(item);
    }

    [HttpPatch("{id:int}/toggle-active")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var item = await _galleryService.ToggleActiveAsync(id);
        if (item == null)
            return NotFound(new { message = $"Gallery item with id {id} was not found." });

        return Ok(item);
    }
}
