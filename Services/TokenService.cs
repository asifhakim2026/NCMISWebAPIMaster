using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NCMIS.Models;
using NCMISAPI.Configuration;

namespace NCMISAPI.Services;

public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;

    public TokenService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public string GenerateAccessToken(UserLogin user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserID.ToString()),
            // Explicit NameIdentifier so controllers still resolve user id if inbound claim mapping differs.
            new(ClaimTypes.NameIdentifier, user.UserID.ToString()),
            new("UserID", user.UserID.ToString()),
            new("UserName", user.UserName),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new("fullName", user.FullName ?? string.Empty),
            new("roleId", user.RoleId.ToString()),
            new("userTypes", user.UserTypes.ToString())
        };

        var credentials = JwtSigning.CreateSigningCredentials(_jwtSettings.Key);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: GetAccessTokenExpiry(),
            signingCredentials: credentials);

        token.Header[JwtHeaderParameterNames.Kid] = JwtSigning.KeyId;

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    public string HashToken(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hash);
    }

    public DateTime GetAccessTokenExpiry() =>
        DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);

    public DateTime GetRefreshTokenExpiry() =>
        DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays);
}
