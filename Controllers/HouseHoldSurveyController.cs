using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NCMISAPI.DTOs;
using NCMISAPI.DTOs.Person;
using NCMISAPI.Services;

namespace NCMISAPI.Controllers;

/// <summary>
/// Housing and Assets survey question bank from GeneralSetups (Type == HouseHold).
/// NOT HouseHold Support — that is api/HouseHoldSupport/categories (SetupHouseHoldCategories).
/// Unhandled exceptions → ExceptionHandlingMiddleware → ErrorLogHelper.
/// </summary>
[Authorize]
[Route("api/HouseHoldSurvey")]
[ApiController]
public class HouseHoldSurveyController : ApiControllerBase
{
    private readonly IHouseHoldSurveyService _houseHoldSurveyService;

    public HouseHoldSurveyController(IHouseHoldSurveyService houseHoldSurveyService)
    {
        _houseHoldSurveyService = houseHoldSurveyService;
    }

    /// <summary>
    /// Question bank for Housing and Assets survey from GeneralSetups
    /// (Type == HouseHold, ParentId == 0 parents + active child options).
    /// Returns drinking water / toilet / land / items / livestock — NOT Food/Health/Housing.
    /// For Food/Health/Housing use GET api/HouseHoldSupport/categories instead.
    /// </summary>
    [HttpGet("categories")]
    [HttpGet("questions")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(ApiResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSurveyCategories(CancellationToken cancellationToken = default)
    {
        var result = await _houseHoldSurveyService.GetSurveyCategoriesAsync(cancellationToken);
        return FromService(result);
    }
}
