using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NCMISAPI.DTOs;
using NCMISAPI.Services;

namespace NCMISAPI.Controllers;

/// <summary>
/// Fee remission APIs for mobile / clients.
/// Unhandled exceptions → ExceptionHandlingMiddleware → ErrorLogHelper.
/// </summary>
[Authorize(Policy = "LoggedInPolicy")]
[Route("api/[controller]")]
[ApiController]
public class FeeRemissionController : ControllerBase
{
    private readonly IFeeRemissionService _feeRemissionService;
    private readonly ILogger<FeeRemissionController> _logger;

    public FeeRemissionController(
        IFeeRemissionService feeRemissionService,
        ILogger<FeeRemissionController> logger)
    {
        _feeRemissionService = feeRemissionService;
        _logger = logger;
    }

    /// <summary>
    /// Returns fee remission cases assigned to the logged-in user
    /// (CurrentAssignToUserID, IsCurrentStepActive).
    /// Optional filters: stepid, schoolid, studentenrollmentnumber, status, casestatus, keywordfilter, page.
    /// When status is omitted, defaults to pending (null CurrentStatus is treated as pending).
    /// </summary>
    [HttpGet("list")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(FeeRemissionListResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> FeeRemissionList(
        [FromQuery] int? stepid,
        [FromQuery] int? schoolid,
        [FromQuery] string? studentenrollmentnumber,
        [FromQuery] string? status,
        [FromQuery] string? casestatus,
        [FromQuery] string? keywordfilter,
        [FromQuery] int page = 1,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId <= 0)
        {
            _logger.LogWarning("FeeRemissionList called without valid user id claim.");
            return Unauthorized(new ApiErrorResponseDto
            {
                Success = false,
                Message = "Unauthorized. Access token is missing a valid user id claim.",
                TraceId = HttpContext.TraceIdentifier,
                StatusCode = StatusCodes.Status401Unauthorized
            });
        }

        var request = new FeeRemissionListRequestDto
        {
            StepId = stepid,
            SchoolId = schoolid,
            StudentEnrollmentNumber = studentenrollmentnumber,
            Status = status,
            CaseStatus = casestatus,
            KeywordFilter = keywordfilter,
            Page = page
        };

        var result = await _feeRemissionService.GetFeeRemissionListAsync(
            userId,
            request,
            cancellationToken);

        return Ok(result);
    }

    private int GetCurrentUserId()
    {
        var raw =
            User.FindFirstValue("UserID")
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(raw, out var userId) ? userId : 0;
    }
}
