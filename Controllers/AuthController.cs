using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NCMISAPI.DTOs;
using NCMISAPI.Services;

namespace NCMISAPI.Controllers;

/// <summary>
/// Authentication endpoints for JWT access and refresh tokens.
/// Unhandled exceptions → ExceptionHandlingMiddleware → ErrorLogHelper.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user and returns access + refresh tokens.
    /// Send deviceInfo from the mobile app so sessions can be listed later.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.DeviceInfo))
        {
            request.DeviceInfo = Request.Headers["X-Device-Info"].FirstOrDefault()
                ?? Request.Headers.UserAgent.FirstOrDefault();
        }

        _logger.LogInformation("Login request received for username {UserName}.", request.UserName);

        var result = await _authService.LoginAsync(request);

        if (!result.Success)
            return Unauthorized(result);

        return Ok(result);
    }

    /// <summary>
    /// Exchanges a valid refresh token for a new access + refresh token pair.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("refresh")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
    {
        _logger.LogInformation("Refresh token request received.");

        var result = await _authService.RefreshTokenAsync(request);

        if (!result.Success)
            return Unauthorized(result);

        return Ok(result);
    }

    /// <summary>
    /// Revokes the refresh token (logout on this device).
    /// </summary>
    [AllowAnonymous]
    [HttpPost("logout")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDto request)
    {
        _logger.LogInformation("Logout request received.");

        var result = await _authService.LogoutAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Returns claims from the current access token — use this to verify Authorization works.
    /// </summary>
    [Authorize(Policy = "LoggedInPolicy")]
    [HttpGet("me")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status401Unauthorized)]
    public IActionResult Me()
    {
        var userId =
            User.FindFirstValue("UserID")
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Ok(new
        {
            success = true,
            message = "Access token is valid.",
            userId,
            userName = User.FindFirstValue("UserName")
                ?? User.FindFirstValue(JwtRegisteredClaimNames.UniqueName),
            claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
        });
    }

    /// <summary>
    /// Returns device/session info for the logged-in user (from RefreshTokens).
    /// </summary>
    [Authorize(Policy = "LoggedInPolicy")]
    [HttpGet("devices")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<DeviceSessionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDevices()
    {
        var userIdClaim = User.FindFirstValue("UserID")
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
        {
            _logger.LogWarning("GetDevices failed: missing or invalid user id claim.");
            return Unauthorized(new ApiErrorResponseDto
            {
                Success = false,
                Message = "Unauthorized. Access token is missing a valid user id claim.",
                TraceId = HttpContext.TraceIdentifier,
                StatusCode = StatusCodes.Status401Unauthorized
            });
        }

        _logger.LogInformation("GetDevices request for user {UserId}.", userId);

        var devices = await _authService.GetDevicesAsync(userId);
        return Ok(devices);
    }
}
