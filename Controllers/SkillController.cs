using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NCMISAPI.DTOs;
using NCMISAPI.Services;

namespace NCMISAPI.Controllers;

/// <summary>
/// Life skills master lookup endpoints.
/// Unhandled exceptions → ExceptionHandlingMiddleware → ErrorLogHelper.
/// </summary>
[Authorize(Policy = "LoggedInPolicy")]
[Route("api/[controller]")]
[ApiController]
public class SkillController : ControllerBase
{
    private readonly ISkillService _skillService;

    public SkillController(ISkillService skillService)
    {
        _skillService = skillService;
    }

    /// <summary>
    /// Returns all rows from LifeSkillsMasters (SkillId, SkillName, Category).
    /// </summary>
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IReadOnlyList<LifeSkillDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
        var skills = await _skillService.GetAllAsync(cancellationToken);
        return Ok(skills);
    }
}
