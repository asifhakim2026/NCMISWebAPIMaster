namespace NCMISAPI.DTOs;

/// <summary>
/// Request to revoke a refresh token (logout).
/// </summary>
public class LogoutRequestDto
{
    /// <summary>
    /// Refresh token to revoke.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;
}
