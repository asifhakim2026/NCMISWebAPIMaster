using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NCMISAPI.Services;

namespace NCMISAPI.Controllers;

/// <summary>
/// PersonEmploymentController - HTTP layer; routes preserved under api/Person.
/// Unhandled exceptions → ExceptionHandlingMiddleware → ErrorLogHelper.
/// </summary>
[Authorize]
[Route("api/Person")]
[ApiController]
public class PersonEmploymentController : ApiControllerBase
{
    private readonly IPersonEmploymentService _employmentService;

    public PersonEmploymentController(IPersonEmploymentService employmentService)
    {
        _employmentService = employmentService;
    }

    [HttpGet("WorkExperienceFamilyGUID")]
    public async Task<IActionResult> WorkExperienceFamilyGUID([FromQuery] Guid FamilyGUID)
    {
        var result = await _employmentService.WorkExperienceFamilyGUID(FamilyGUID);
        return FromService(result);
    }

    [HttpPost("SaveWorkExperience")]
    public async Task<IActionResult> SaveWorkExperience([FromForm] int personId, [FromForm] string? designation, [FromForm] decimal? incomePerMonth, [FromForm] string? fromDate, [FromForm] string? toDate, [FromForm] string? isOngoing, [FromForm] string? employerName, [FromForm] string? employerAddress, [FromForm] string? responsibilities, [FromForm] string? incomeComponentsJson)
    {
        var result = await _employmentService.SaveWorkExperience(personId, designation, incomePerMonth, fromDate, toDate, isOngoing, employerName, employerAddress, responsibilities, incomeComponentsJson);
        return FromService(result);
    }

    [HttpPost("MarkExperienceAsInactive")]
    public async Task<IActionResult> MarkExperienceAsInactive([FromForm] int workExperienceId, [FromForm] string reason, [FromForm] string? description)
    {
        var result = await _employmentService.MarkExperienceAsInactive(workExperienceId, reason, description);
        return FromService(result);
    }
}
