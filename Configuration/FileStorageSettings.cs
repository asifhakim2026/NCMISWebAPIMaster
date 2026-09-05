namespace NCMISAPI.Configuration;

/// <summary>
/// Shared file storage. Point RootPath at the other app's cdn folder so both apps use the same pictures.
/// </summary>
public class FileStorageSettings
{
    public const string SectionName = "FileStorage";

    /// <summary>
    /// Absolute path to the folder where files are stored (e.g. the MVC wwwroot\cdn folder).
    /// When empty, falls back to {API WebRoot}/cdn.
    /// </summary>
    public string? RootPath { get; set; }

    /// <summary>
    /// Public URL prefix for saved files (default /cdn).
    /// </summary>
    public string RequestPath { get; set; } = "/cdn";
}
