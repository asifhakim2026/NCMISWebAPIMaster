using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NCMISAPI.DTOs;
using NCMISAPI.DTOs.Person;
using NCMISAPI.Services;

namespace NCMISAPI.Controllers;

/// <summary>
/// Additional Support module APIs (SupportSurvey categories from GeneralSetups).
/// Distinct from HouseHold Support (SetupHouseHoldCategories) and from the
/// family-scoped survey load at api/Person/AddtionalSupportsurveyload.
/// Unhandled exceptions → ExceptionHandlingMiddleware → ErrorLogHelper.
/// </summary>
[Authorize(Policy = "LoggedInPolicy")]
[Route("api/[controller]")]
[ApiController]
public class AdditionalSupportController : ApiControllerBase
{
    private readonly IAdditionalSupportService _additionalSupportService;

    public AdditionalSupportController(IAdditionalSupportService additionalSupportService)
    {
        _additionalSupportService = additionalSupportService;
    }

    /// <summary>
    /// Fill-list for Additional Support forms: active SupportSurvey parent categories
    /// from GeneralSetups with active child options (no familyGuid required).
    /// </summary>
    [HttpGet("categories")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSupportCategories(CancellationToken cancellationToken = default)
    {
        var result = await _additionalSupportService.GetSupportCategoriesAsync(cancellationToken);
        return FromService(result);
    }
}
