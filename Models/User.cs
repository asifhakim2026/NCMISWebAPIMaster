namespace NCMISAPI.Models;

public class User
{
    public int UserId { get; set; }

    public Guid UserGuid { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public string Email { get; set; } = string.Empty;

    public int UserTypes { get; set; }

    public int RoleId { get; set; }

    public bool IsViewer { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public int FailedLoginAttempts { get; set; }

    public DateTime? LockoutEndTime { get; set; }
}
