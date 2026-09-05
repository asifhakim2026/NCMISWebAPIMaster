using Microsoft.EntityFrameworkCore;
using NCMIS.Helpers;
using NCMIS.Models;
using NCMISAPI.Data;
using NCMISAPI.DTOs;

namespace NCMISAPI.Services;

public class AuthService : IAuthService
{
    private readonly NcmisDbContext _dbContext;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        NcmisDbContext dbContext,
        ITokenService tokenService,
        ILogger<AuthService> logger)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
        {
            _logger.LogWarning("Login rejected: missing username or password.");
            return Fail("Username and password are required.");
        }

        var user = await _dbContext.UserLogins
            .FirstOrDefaultAsync(u => u.UserName == request.UserName);

        if (user is null)
        {
            _logger.LogWarning("Login failed for username {UserName}: user not found.", request.UserName);
            return Fail("Invalid username or password.");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login failed for user {UserId}: inactive account.", user.UserID);
            return Fail("User account is inactive.");
        }

        if (user.ExpiryDate.HasValue && user.ExpiryDate.Value < DateTime.UtcNow)
        {
            _logger.LogWarning("Login failed for user {UserId}: account expired.", user.UserID);
            return Fail("User account has expired.");
        }

        if (user.LockoutEndTime.HasValue && user.LockoutEndTime.Value > DateTime.UtcNow)
        {
            _logger.LogWarning("Login failed for user {UserId}: account locked.", user.UserID);
            return Fail("User account is locked. Try again later.");
        }

        if (!string.Equals(EncryptionHelper.dycryption(user.Password), request.Password, StringComparison.Ordinal))
        {
            _logger.LogWarning("Login failed for user {UserId}: invalid password.", user.UserID);
            return Fail("Invalid username or password.");
        }

        var result = await IssueTokensAsync(user, request.DeviceInfo);
        _logger.LogInformation(
            "Login successful for user {UserId} ({UserName}). DeviceInfo={DeviceInfo}",
            user.UserID,
            user.UserName,
            request.DeviceInfo ?? "(none)");

        return result;
    }

    public async Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            _logger.LogWarning("Refresh rejected: missing refresh token.");
            return Fail("Refresh token is required.");
        }

        var tokenHash = _tokenService.HashToken(request.RefreshToken);

        var storedToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

        if (storedToken is null)
        {
            _logger.LogWarning("Refresh failed: token not found.");
            return Fail("Invalid refresh token.");
        }

        if (storedToken.IsRevoked)
        {
            _logger.LogWarning(
                "Refresh failed for user {UserId}: token revoked.",
                storedToken.UserId);
            return Fail("Refresh token has been revoked.");
        }

        if (storedToken.IsExpired)
        {
            _logger.LogWarning(
                "Refresh failed for user {UserId}: token expired.",
                storedToken.UserId);
            return Fail("Refresh token has expired. Please login again.");
        }

        var user = await _dbContext.UserLogins
            .FirstOrDefaultAsync(u => u.UserID == storedToken.UserId);

        if (user is null || !user.IsActive)
        {
            _logger.LogWarning(
                "Refresh failed for user {UserId}: account unavailable.",
                storedToken.UserId);
            return Fail("User account is not available.");
        }

        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var newRefreshHash = _tokenService.HashToken(newRefreshToken);
        var refreshExpiresAt = _tokenService.GetRefreshTokenExpiry();

        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.ReplacedByTokenHash = newRefreshHash;

        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.UserID,
            TokenHash = newRefreshHash,
            ExpiresAt = refreshExpiresAt,
            CreatedAt = DateTime.UtcNow,
            DeviceInfo = storedToken.DeviceInfo
        });

        await _dbContext.SaveChangesAsync();

        var accessToken = _tokenService.GenerateAccessToken(user);
        var accessExpiresAt = _tokenService.GetAccessTokenExpiry();

        _logger.LogInformation("Token refreshed for user {UserId}.", user.UserID);

        return new LoginResponseDto
        {
            Success = true,
            Message = "Token refreshed.",
            Token = accessToken,
            RefreshToken = newRefreshToken,
            AccessTokenExpiresAt = accessExpiresAt,
            RefreshTokenExpiresAt = refreshExpiresAt,
            UserId = user.UserID,
            Username = user.UserName
        };
    }

    public async Task<LoginResponseDto> LogoutAsync(LogoutRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            _logger.LogWarning("Logout rejected: missing refresh token.");
            return Fail("Refresh token is required.");
        }

        var tokenHash = _tokenService.HashToken(request.RefreshToken);

        var storedToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

        if (storedToken is null || storedToken.IsRevoked)
        {
            _logger.LogInformation("Logout completed: token already inactive.");
            return new LoginResponseDto
            {
                Success = true,
                Message = "Logged out."
            };
        }

        storedToken.RevokedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Logout successful for user {UserId}.", storedToken.UserId);

        return new LoginResponseDto
        {
            Success = true,
            Message = "Logged out."
        };
    }

    public async Task<IReadOnlyList<DeviceSessionDto>> GetDevicesAsync(int userId)
    {
        var devices = await _dbContext.RefreshTokens
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new DeviceSessionDto
            {
                Id = t.Id,
                DeviceInfo = t.DeviceInfo,
                CreatedAt = t.CreatedAt,
                ExpiresAt = t.ExpiresAt,
                RevokedAt = t.RevokedAt,
                IsActive = t.RevokedAt == null && t.ExpiresAt > DateTime.UtcNow
            })
            .ToListAsync();

        _logger.LogInformation(
            "Retrieved {Count} device sessions for user {UserId}.",
            devices.Count,
            userId);

        return devices;
    }

    private async Task<LoginResponseDto> IssueTokensAsync(UserLogin user, string? deviceInfo = null)
    {
        var accessToken = _tokenService.GenerateAccessToken(user);
        var accessExpiresAt = _tokenService.GetAccessTokenExpiry();

        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshHash = _tokenService.HashToken(refreshToken);
        var refreshExpiresAt = _tokenService.GetRefreshTokenExpiry();

        var normalizedDeviceInfo = string.IsNullOrWhiteSpace(deviceInfo)
            ? null
            : deviceInfo.Trim();

        if (normalizedDeviceInfo is { Length: > 200 })
            normalizedDeviceInfo = normalizedDeviceInfo[..200];

        _dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.UserID,
            TokenHash = refreshHash,
            ExpiresAt = refreshExpiresAt,
            CreatedAt = DateTime.UtcNow,
            DeviceInfo = normalizedDeviceInfo
        });

        user.LastLogin = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return new LoginResponseDto
        {
            Success = true,
            Message = "Login successful.",
            Token = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAt = accessExpiresAt,
            RefreshTokenExpiresAt = refreshExpiresAt,
            UserId = user.UserID,
            Username = user.UserName
        };
    }

    private static LoginResponseDto Fail(string message) => new()
    {
        Success = false,
        Message = message
    };
}
