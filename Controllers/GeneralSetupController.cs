using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NCMISAPI.DTOs;
using NCMISAPI.Services;

namespace NCMISAPI.Controllers;

/// <summary>
/// GeneralSetup lookup endpoints (Income / Expense options).
/// Unhandled exceptions → ExceptionHandlingMiddleware → ErrorLogHelper.
/// </summary>
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class GeneralSetupController : ControllerBase
{
    private readonly IGeneralSetupService _generalSetupService;

    public GeneralSetupController(IGeneralSetupService generalSetupService)
    {
        _generalSetupService = generalSetupService;
    }

    /// <summary>
    /// Active Income option rows from GeneralSetup (children of Income roots).
    /// </summary>
    [HttpGet("income")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IReadOnlyList<GeneralSetupLookupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetIncome(CancellationToken cancellationToken = default)
    {
        var items = await _generalSetupService.GetIncomeItemsAsync(cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Active Expense option rows from GeneralSetup (children of Expense roots).
    /// </summary>
    [HttpGet("expense")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IReadOnlyList<GeneralSetupLookupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetExpense(CancellationToken cancellationToken = default)
    {
        var items = await _generalSetupService.GetExpenseItemsAsync(cancellationToken);
        return Ok(items);
    }
}
