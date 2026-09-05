namespace NCMISAPI.DTOs;

/// <summary>Open file stream for a secured download.</summary>
public sealed class StoredFileResult : IAsyncDisposable, IDisposable
{
    public required Stream Stream { get; init; }
    public required string FileName { get; init; }
    public required string ContentType { get; init; }

    public void Dispose() => Stream.Dispose();

    public ValueTask DisposeAsync() => Stream.DisposeAsync();
}
