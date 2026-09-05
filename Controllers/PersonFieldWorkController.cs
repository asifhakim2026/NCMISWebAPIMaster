using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NCMISAPI.Services;

namespace NCMISAPI.Controllers;

/// <summary>
/// PersonFieldWorkController - HTTP layer; routes preserved under api/Person.
/// Unhandled exceptions → ExceptionHandlingMiddleware → ErrorLogHelper.
/// </summary>
[Authorize(Policy = "LoggedInPolicy")]
[Route("api/Person")]
[ApiController]
public class PersonFieldWorkController : ApiControllerBase
{
    private readonly IPersonFieldWorkService _fieldWorkService;

    public PersonFieldWorkController(IPersonFieldWorkService fieldWorkService)
    {
        _fieldWorkService = fieldWorkService;
    }

    [HttpGet("SurveyorNotesFamilyGUID")]
    public async Task<IActionResult> SurveyorNotesFamilyGUID([FromQuery] Guid FamilyGUID)
    {
        var result = await _fieldWorkService.SurveyorNotesFamilyGUID(FamilyGUID);
        return FromService(result);
    }

    [HttpPost("SurveyNotesFamily")]
    public async Task<IActionResult> SurveyNotesFamily([FromForm] int familyid, [FromForm] string notes, [FromForm] decimal? latitude, [FromForm] decimal? longitude, [FromForm] string? imagePath, [FromForm] string? address)
    {
        var result = await _fieldWorkService.SurveyNotesFamily(familyid, notes, latitude, longitude, imagePath, address);
        return FromService(result);
    }

    [HttpGet("GetSurveyorFamilyNotes")]
    public async Task<IActionResult> GetSurveyorFamilyNotes([FromQuery] int familyId)
    {
        var result = await _fieldWorkService.GetSurveyorFamilyNotes(familyId);
        return FromService(result);
    }

    [HttpGet("GetFamilyVerificationSummary")]
    public async Task<IActionResult> GetFamilyVerificationSummary([FromQuery] Guid FamilyGUID)
    {
        var result = await _fieldWorkService.GetFamilyVerificationSummary(FamilyGUID);
        return FromService(result);
    }

    [HttpPost("SaveFamilyVerification")]
    public async Task<IActionResult> SaveFamilyVerification([FromForm] string? SignatureBase64, [FromForm] int FamilyId, [FromForm] string SignedBy, [FromForm] string VerifiedDataJson)
    {
        var result = await _fieldWorkService.SaveFamilyVerification(SignatureBase64, FamilyId, SignedBy, VerifiedDataJson);
        return FromService(result);
    }

    [HttpGet("ViewFamilyVerification")]
    public async Task<IActionResult> ViewFamilyVerification([FromQuery] Guid FamilyGUID)
    {
        var result = await _fieldWorkService.ViewFamilyVerification(FamilyGUID);
        return FromService(result);
    }
}
