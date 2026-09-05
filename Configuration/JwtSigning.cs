using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace NCMISAPI.Configuration;

/// <summary>
/// Builds the shared HMAC signing key used for both token creation and validation.
/// </summary>
public static class JwtSigning
{
    /// <summary>
    /// Stable kid so JsonWebTokenHandler (ASP.NET Core 8+/9 default) can resolve the symmetric key.
    /// </summary>
    public const string KeyId = "ncmis-hmac-sha256";

    public static SymmetricSecurityKey CreateSecurityKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Length < 32)
            throw new InvalidOperationException(
                "Jwt:Key must be configured and at least 32 characters.");

        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        {
            KeyId = KeyId
        };
    }

    public static SigningCredentials CreateSigningCredentials(string key) =>
        new(CreateSecurityKey(key), SecurityAlgorithms.HmacSha256);
}
