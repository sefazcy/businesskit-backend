using BusinessKit.Application.Uploads;
using BusinessKit.Application.Uploads.Dtos;

namespace BusinessKit.Infrastructure.Uploads;

public class LocalFileUploadService : IFileUploadService
{
    private readonly string _uploadDirectory;

    public LocalFileUploadService(string uploadDirectory)
    {
        _uploadDirectory = uploadDirectory;
    }

    public async Task<FileUploadResponseDto> UploadImageAsync(
        Stream fileStream, string originalFileName, string contentType, long size)
    {
        var ext = Path.GetExtension(originalFileName).ToLowerInvariant();
        var safeFileName = $"{Guid.NewGuid():N}{ext}";

        Directory.CreateDirectory(_uploadDirectory);

        var physicalPath = Path.Combine(_uploadDirectory, safeFileName);

        await using var destination = new FileStream(physicalPath, FileMode.CreateNew, FileAccess.Write);
        await fileStream.CopyToAsync(destination);

        return new FileUploadResponseDto
        {
            FileName = safeFileName,
            OriginalFileName = Path.GetFileName(originalFileName),
            ContentType = contentType,
            Size = size,
            Url = $"/uploads/images/{safeFileName}"
        };
    }
}
