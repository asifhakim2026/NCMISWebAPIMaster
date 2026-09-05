namespace NCMISAPI.DTOs;

/// <summary>
/// Request to exchange a refresh token for new tokens.
/// </summary>
public class RefreshTokenRequestDto
{
    /// <summary>
    /// Refresh token previously issued at login/refresh.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
}
