//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using NCMISAPI.Services;

//namespace NCMISAPI.Controllers;

///// <summary>
///// PersonDeceasedController - HTTP layer; routes preserved under api/Person.
///// Unhandled exceptions → ExceptionHandlingMiddleware → ErrorLogHelper.
///// </summary>
//[Authorize(Policy = "LoggedInPolicy")]
//[Route("api/Person")]
//[ApiController]
//public class PersonDeceasedController : ApiControllerBase
//{
//    private readonly IPersonDeceasedService _deceasedService;

//    public PersonDeceasedController(IPersonDeceasedService deceasedService)
//    {
//        _deceasedService = deceasedService;
//    }

//    [HttpGet("LoadDeceasedFormByCNIC")]
//    public async Task<IActionResult> LoadDeceasedFormByCNIC([FromQuery] string cnic)
//    {
//        var result = await _deceasedService.LoadDeceasedFormByCNIC(cnic);
//        return FromService(result);
//    }

//    [HttpPost("InsertPersonDeceased")]
//    public async Task<IActionResult> InsertPersonDeceased([FromForm] int PersonId, [FromForm] DateTime? DateOfDeath, [FromForm] string TimeOfDeath, [FromForm] string PlaceOfDeath, [FromForm] string ReportedByName, [FromForm] string ReportedByRelation, [FromForm] int? CauseOfDeathTypeId, [FromForm] int? GraveyardId, [FromForm] string DeathPrayerCenter, [FromForm] string? HealthConditionIdsCsv, [FromForm] string? DeathCertificateFilePath, [FromForm] string? AdditionalRemarks)
//    {
//        var result = await _deceasedService.InsertPersonDeceased(PersonId, DateOfDeath, TimeOfDeath, PlaceOfDeath, ReportedByName, ReportedByRelation, CauseOfDeathTypeId, GraveyardId, DeathPrayerCenter, HealthConditionIdsCsv, DeathCertificateFilePath, AdditionalRemarks);
//        return FromService(result);
//    }

//    [HttpGet("DeceasedList")]
//    public async Task<IActionResult> DeceasedList([FromQuery] int page = 1, [FromQuery] string keyword = "", [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null, [FromQuery] bool useDeathDate = false)
//    {
//        var result = await _deceasedService.DeceasedList(page, keyword, fromDate, toDate, useDeathDate);
//        return FromService(result);
//    }

//    [HttpGet("DeceasedAnalysisThisYear")]
//    public async Task<IActionResult> DeceasedAnalysisThisYear()
//    {
//        var result = await _deceasedService.DeceasedAnalysisThisYear();
//        return FromService(result);
//    }

//    [HttpPost("SaveDeceasedInfoSimple")]
//    public IActionResult SaveDeceasedInfoSimple([FromForm] string deceasedFirstName, [FromForm] string deceasedFatherName, [FromForm] string? deceasedLastName, [FromForm] string deceasedCNIC, [FromForm] string deceasedIdentificationType, [FromForm] int dobDay, [FromForm] int dobMonth, [FromForm] int dobYear, [FromForm] string gender, [FromForm] int jkId, [FromForm] string completeAddress, [FromForm] string? lat, [FromForm] string? lon, [FromForm] string relativeFirstName, [FromForm] string relativeFatherName, [FromForm] string? relativeLastName, [FromForm] string relativeGender, [FromForm] string? relativeMaritalStatus, [FromForm] int relativedobDay, [FromForm] int relativedobMonth, [FromForm] int relativedobYear, [FromForm] string relativeCNIC, [FromForm] string relativeIdentificationType, [FromForm] int relationshipTypeId, [FromForm] string phoneNumber, [FromForm] string emailAddress)
//    {
//        var result = _deceasedService.SaveDeceasedInfoSimple(deceasedFirstName, deceasedFatherName, deceasedLastName, deceasedCNIC, deceasedIdentificationType, dobDay, dobMonth, dobYear, gender, jkId, completeAddress, lat, lon, relativeFirstName, relativeFatherName, relativeLastName, relativeGender, relativeMaritalStatus, relativedobDay, relativedobMonth, relativedobYear, relativeCNIC, relativeIdentificationType, relationshipTypeId, phoneNumber, emailAddress);
//        return FromService(result);
//    }
//}
