using NCMIS.Models;

namespace NCMISAPI.Services;

public interface ITokenService
{
    string GenerateAccessToken(UserLogin user);

    string GenerateRefreshToken();

    string HashToken(string token);

    DateTime GetAccessTokenExpiry();

    DateTime GetRefreshTokenExpiry();
}
