namespace NCMISAPI.DTOs;

/// <summary>
/// Response after a successful file upload.
/// </summary>
public class FileUploadResponseDto
{
    public bool Success { get; set; }

    public string FilePath { get; set; } = string.Empty;
}
