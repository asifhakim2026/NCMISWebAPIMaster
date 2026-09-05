using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NCMISAPI.Services;

namespace NCMISAPI.Controllers;

/// <summary>
/// PersonEducationController - HTTP layer; routes preserved under api/Person.
/// Unhandled exceptions → ExceptionHandlingMiddleware → ErrorLogHelper.
/// </summary>
[Authorize(Policy = "LoggedInPolicy")]
[Route("api/Person")]
[ApiController]
public class PersonEducationController : ApiControllerBase
{
    private readonly IPersonEducationService _educationService;

    public PersonEducationController(IPersonEducationService educationService)
    {
        _educationService = educationService;
    }

    [HttpGet("EducationFamilyGUID")]
    public async Task<IActionResult> EducationFamilyGUID([FromQuery] Guid FamilyGUID)
    {
        var result = await _educationService.EducationFamilyGUID(FamilyGUID);
        return FromService(result);
    }

    [HttpPost("SaveEducation")]
    public async Task<IActionResult> SaveEducation([FromForm] int personId, [FromForm] string institutionName, [FromForm] string boardType, [FromForm] string board, [FromForm] string group, [FromForm] string degreeType, [FromForm] string fieldOfStudy, [FromForm] string? courseDuration, [FromForm] string? startDate, [FromForm] string? endDate, [FromForm] string? passingDate, [FromForm] int? totalMarks, [FromForm] int? obtainedMarks, [FromForm] string? remarks, [FromForm] string? fundingJson, [FromForm] bool isOngoing = false, [FromForm] bool notknown = false)
    {
        var result = await _educationService.SaveEducation(personId, institutionName, boardType, board, group, degreeType, fieldOfStudy, courseDuration, startDate, endDate, passingDate, totalMarks, obtainedMarks, remarks, fundingJson, isOngoing, notknown);
        return FromService(result);
    }

    [HttpPost("MarkEducationAsInactive")]
    public async Task<IActionResult> MarkEducationAsInactive([FromForm] int educationId, [FromForm] string reason, [FromForm] string? description)
    {
        var result = await _educationService.MarkEducationAsInactive(educationId, reason, description);
        return FromService(result);
    }

    [HttpGet("YouthEducationFamilyGUID")]
    public async Task<IActionResult> YouthEducationFamilyGUID([FromQuery] Guid FamilyGUID)
    {
        var result = await _educationService.YouthEducationFamilyGUID(FamilyGUID);
        return FromService(result);
    }

    [HttpPost("SaveYouthEducation")]
    public async Task<IActionResult> SaveYouthEducation([FromForm] int personId, [FromForm] string className, [FromForm] string centerName, [FromForm] string completionStatus, [FromForm] string? completionDate)
    {
        var result = await _educationService.SaveYouthEducation(personId, className, centerName, completionStatus, completionDate);
        return FromService(result);
    }

    [HttpPost("MarkAsInactive")]
    public async Task<IActionResult> MarkAsInactive([FromForm] int educationId, [FromForm] string reason, [FromForm] string? description)
    {
        var result = await _educationService.MarkAsInactive(educationId, reason, description);
        return FromService(result);
    }

    [HttpGet("LifeSkillsFamilyGUID")]
    public async Task<IActionResult> LifeSkillsFamilyGUID([FromQuery] Guid FamilyGUID)
    {
        var result = await _educationService.LifeSkillsFamilyGUID(FamilyGUID);
        return FromService(result);
    }

    [HttpPost("SaveSkillWithDetails")]
    public async Task<IActionResult> SaveSkillWithDetails([FromForm] int PersonId, [FromForm] int SkillId, [FromForm] bool IsCertified, [FromForm] string? Proficiency, [FromForm] bool IsChecked)
    {
        var result = await _educationService.SaveSkillWithDetails(PersonId, SkillId, IsCertified, Proficiency, IsChecked);
        return FromService(result);
    }

    [HttpPost("AddNewSkill")]
    public async Task<IActionResult> AddNewSkill([FromForm] string SkillName, [FromForm] string Category)
    {
        var result = await _educationService.AddNewSkill(SkillName, Category);
        return FromService(result);
    }
}
