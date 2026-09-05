namespace NCMISAPI.DTOs;

/// <summary>
/// A device/session linked to a refresh token.
/// </summary>
public class DeviceSessionDto
{
    public int Id { get; set; }

    public string? DeviceInfo { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public bool IsActive { get; set; }
}
