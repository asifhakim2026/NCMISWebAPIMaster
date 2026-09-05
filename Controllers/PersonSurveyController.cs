using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NCMISAPI.DTOs.Person;
using NCMISAPI.Services;

namespace NCMISAPI.Controllers;

/// <summary>
/// PersonSurveyController - HTTP layer; routes preserved under api/Person.
/// Unhandled exceptions → ExceptionHandlingMiddleware → ErrorLogHelper.
/// </summary>
[Authorize]
[Route("api/Person")]
[ApiController]
public class PersonSurveyController : ApiControllerBase
{
    private readonly IPersonSurveyService _surveyService;

    public PersonSurveyController(IPersonSurveyService surveyService)
    {
        _surveyService = surveyService;
    }

    /// <summary>
    /// Family-scoped Housing and Assets survey load (questions from GeneralSetups
    /// Type == HouseHold). Requires familyGuid for family context; the question bank
    /// itself does not depend on the family — use GET api/HouseHoldSurvey/categories
    /// when only the question/options list is needed.
    /// </summary>
    [HttpGet("HouseholdSurvey")]
    public async Task<IActionResult> HouseholdSurvey([FromQuery] Guid familyGuid)
    {
        var result = await _surveyService.HouseholdSurvey(familyGuid);
        return FromService(result);
    }

    [HttpPost("SaveHouseHoldSurveyResponse")]
    public async Task<IActionResult> SaveHouseHoldSurveyResponse([FromBody] SurveyRequestDto model, [FromQuery] Guid familyGuid)
    {
        var result = await _surveyService.SaveHouseHoldSurveyResponse(model, familyGuid);
        return FromService(result);
    }

    [HttpGet("HouseholdSurveyAnalysis")]
    public async Task<IActionResult> HouseholdSurveyAnalysis([FromQuery] Guid familyGuid)
    {
        var result = await _surveyService.HouseholdSurveyAnalysis(familyGuid);
        return FromService(result);
    }

    [HttpGet("incomeexpenseload")]
    public async Task<IActionResult> incomeexpenseload([FromQuery] Guid familyGuid)
    {
        var result = await _surveyService.incomeexpenseload(familyGuid);
        return FromService(result);
    }

    [HttpPost("SaveIncomeExpense")]
    public async Task<IActionResult> SaveIncomeExpense([FromBody] SurveyRequestDto model, [FromQuery] Guid familyGuid)
    {
        var result = await _surveyService.SaveIncomeExpense(model, familyGuid);
        return FromService(result);
    }

    [HttpGet("IncomeExpenseSurveyAnalysis")]
    public async Task<IActionResult> IncomeExpenseSurveyAnalysis([FromQuery] Guid familyGuid)
    {
        var result = await _surveyService.IncomeExpenseSurveyAnalysis(familyGuid);
        return FromService(result);
    }

    [HttpGet("AddtionalSupportsurveyload")]
    public async Task<IActionResult> AddtionalSupportsurveyload([FromQuery] Guid familyGuid)
    {
        var result = await _surveyService.AddtionalSupportsurveyload(familyGuid);
        return FromService(result);
    }

    [HttpPost("SaveSupportSurveyResponse")]
    public async Task<IActionResult> SaveSupportSurveyResponse([FromBody] SurveyRequestDto model, [FromQuery] Guid familyGuid)
    {
        var result = await _surveyService.SaveSupportSurveyResponse(model, familyGuid);
        return FromService(result);
    }

    [HttpGet("AddtionalSupportSurveyAnalysis")]
    public async Task<IActionResult> AddtionalSupportSurveyAnalysis([FromQuery] Guid FamilyGUID)
    {
        var result = await _surveyService.AddtionalSupportSurveyAnalysis(FamilyGUID);
        return FromService(result);
    }
}
