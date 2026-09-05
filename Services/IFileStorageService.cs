using NCMISAPI.DTOs;

namespace NCMISAPI.Services;

public interface IFileStorageService
{
    Task<FileUploadResponseDto> SaveFileAsync(IFormFile file, CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens a file from the configured CDN folder for download.
    /// Accepts "/cdn/name.jpg", "cdn/name.jpg", or "name.jpg".
    /// Returns null when the file does not exist.
    /// </summary>
    Task<StoredFileResult?> OpenReadAsync(string pathOrFileName, CancellationToken cancellationToken = default);
}
