using BusinessKit.Application.Uploads;
using BusinessKit.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace BusinessKit.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/uploads")]
[Authorize(Roles = Roles.Admin)]
[Tags("Uploads (Admin)")]
public class AdminUploadsController : ControllerBase
{
    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };

    private static readonly HashSet<string> AllowedContentTypes =
        new(StringComparer.OrdinalIgnoreCase) { "image/jpeg", "image/png", "image/webp" };

    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    private readonly IFileUploadService _fileUploadService;

    public AdminUploadsController(IFileUploadService fileUploadService)
    {
        _fileUploadService = fileUploadService;
    }

    [HttpPost("image")]
    [Consumes("multipart/form-data")]
    [RequestFormLimits(MultipartBodyLengthLimit = 6 * 1024 * 1024)]
    public async Task<IActionResult> UploadImage([FromForm] IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "A non-empty file is required." });

        if (file.Length > MaxFileSizeBytes)
            return BadRequest(new { message = "File size must not exceed 5 MB." });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            return BadRequest(new { message = $"Extension '{ext}' is not allowed. Allowed: .jpg, .jpeg, .png, .webp" });

        if (!AllowedContentTypes.Contains(file.ContentType))
            return BadRequest(new { message = $"Content type '{file.ContentType}' is not allowed. Allowed: image/jpeg, image/png, image/webp" });

        using var stream = file.OpenReadStream();
        var result = await _fileUploadService.UploadImageAsync(
            stream, file.FileName, file.ContentType, file.Length);

        return Ok(result);
    }
}
