using BusinessKit.Application.Uploads.Dtos;

namespace BusinessKit.Application.Uploads;

public interface IFileUploadService
{
    Task<FileUploadResponseDto> UploadImageAsync(Stream fileStream, string originalFileName, string contentType, long size);
}
