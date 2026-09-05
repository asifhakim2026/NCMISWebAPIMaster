namespace NCMISAPI.DTOs;

/// <summary>
/// Result of a login or token refresh.
/// </summary>
public class LoginResponseDto
{
    /// <summary>
    /// Whether the operation succeeded.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// Human-readable status message.
    /// </summary>
    /// <example>Login successful.</example>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Short-lived JWT access token.
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// Long-lived refresh token for mobile apps.
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// UTC expiry of the access token.
    /// </summary>
    public DateTime? AccessTokenExpiresAt { get; set; }

    /// <summary>
    /// UTC expiry of the refresh token.
    /// </summary>
    public DateTime? RefreshTokenExpiresAt { get; set; }

    /// <summary>
    /// Authenticated user ID.
    /// </summary>
    /// <example>1</example>
    public int? UserId { get; set; }

    /// <summary>
    /// Authenticated username.
    /// </summary>
    /// <example>admin</example>
    public string? Username { get; set; }
}
