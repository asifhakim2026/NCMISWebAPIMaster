namespace NCMISAPI.DTOs;

/// <summary>
/// Login credentials for JWT authentication.
/// </summary>
public class LoginRequestDto
{
    /// <summary>
    /// Username from the UserLogins table.
    /// </summary>
    /// <example>admin</example>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// User password.
    /// </summary>
    /// <example>your-password</example>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Optional device details from the mobile app (model, OS, app version).
    /// </summary>
    /// <example>Samsung SM-A525F | Android 14 | App 1.2.0</example>
    public string? DeviceInfo { get; set; }
}
