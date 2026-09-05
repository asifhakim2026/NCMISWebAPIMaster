namespace NCMISAPI.DTOs;

/// <summary>
/// Multipart form payload for file upload.
/// </summary>
public class FileUploadFormDto
{
    public IFormFile File { get; set; } = null!;
}
