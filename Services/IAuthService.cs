using NCMISAPI.DTOs;

namespace NCMISAPI.Services;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);

    Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);

    Task<LoginResponseDto> LogoutAsync(LogoutRequestDto request);

    Task<IReadOnlyList<DeviceSessionDto>> GetDevicesAsync(int userId);
}
