using Microsoft.Extensions.Options;
using NCMISAPI.Configuration;
using NCMISAPI.DTOs;

namespace NCMISAPI.Services;

public class FileStorageService : IFileStorageService
{
    private const string DefaultSubFolder = "cdn";
    private readonly IWebHostEnvironment _environment;
    private readonly FileStorageSettings _settings;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(
        IWebHostEnvironment environment,
        IOptions<FileStorageSettings> settings,
        ILogger<FileStorageService> logger)
    {
        _environment = environment;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<FileUploadResponseDto> SaveFileAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("No file uploaded.");

        var uploadsFolder = ResolveStorageRoot();
        Directory.CreateDirectory(uploadsFolder);

        var fileExtension = Path.GetExtension(file.FileName);
        var uniqueFileName = Path.GetFileNameWithoutExtension(file.FileName)
            + "_" + Guid.NewGuid().ToString("N")
            + fileExtension;

        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        await using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream, cancellationToken);
        }

        var requestPath = NormalizeRequestPath(_settings.RequestPath);
        var relativePath = $"{requestPath}/{uniqueFileName}";

        _logger.LogInformation(
            "File uploaded: {FilePath} -> {PhysicalPath} ({Bytes} bytes).",
            relativePath,
            filePath,
            file.Length);

        return new FileUploadResponseDto
        {
            Success = true,
            FilePath = relativePath
        };
    }

    public Task<StoredFileResult?> OpenReadAsync(string pathOrFileName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(pathOrFileName))
            throw new ArgumentException("File path is required.");

        var fileName = ExtractSafeFileName(pathOrFileName);
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("Invalid file path.");

        var root = ResolveStorageRoot();
        var fullPath = Path.GetFullPath(Path.Combine(root, fileName));

        var rootFull = Path.GetFullPath(root)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(Path.GetDirectoryName(fullPath), Path.GetFullPath(root), StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Invalid file path.");
        }

        if (!System.IO.File.Exists(fullPath))
            return Task.FromResult<StoredFileResult?>(null);

        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var contentType = GetContentType(fileName);

        _logger.LogInformation("File download opened: {FileName} ({ContentType})", fileName, contentType);

        return Task.FromResult<StoredFileResult?>(new StoredFileResult
        {
            Stream = stream,
            FileName = fileName,
            ContentType = contentType
        });
    }

    public string ResolveStorageRoot()
    {
        if (!string.IsNullOrWhiteSpace(_settings.RootPath))
            return Path.GetFullPath(_settings.RootPath.Trim());

        var webRoot = !string.IsNullOrWhiteSpace(_environment.WebRootPath)
            ? _environment.WebRootPath
            : Path.Combine(_environment.ContentRootPath, "wwwroot");

        return Path.Combine(webRoot, DefaultSubFolder);
    }

    private static string ExtractSafeFileName(string pathOrFileName)
    {
        var value = pathOrFileName.Trim().Replace('\\', '/');
        var segments = value.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return segments.Length == 0 ? string.Empty : Path.GetFileName(segments[^1]);
    }

    private static string GetContentType(string fileName)
    {
        return Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            ".pdf" => "application/pdf",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".xls" => "application/vnd.ms-excel",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"
        };
    }

    private static string NormalizeRequestPath(string? requestPath)
    {
        if (string.IsNullOrWhiteSpace(requestPath))
            return "/" + DefaultSubFolder;

        var path = requestPath.Trim().Replace('\\', '/');
        if (!path.StartsWith('/'))
            path = "/" + path;
        return path.TrimEnd('/');
    }
}
