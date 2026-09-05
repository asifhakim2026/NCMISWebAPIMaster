namespace NCMISAPI.DTOs;

/// <summary>
/// Standard API error payload for mobile and Swagger clients.
/// </summary>
public class ApiErrorResponseDto
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public string? TraceId { get; set; }

    public int StatusCode { get; set; }

    public IDictionary<string, string[]>? Errors { get; set; }
}
