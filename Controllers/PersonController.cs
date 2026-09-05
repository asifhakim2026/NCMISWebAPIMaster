using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NCMISAPI.Services;

namespace NCMISAPI.Controllers;

/// <summary>
/// PersonController - HTTP layer; routes preserved under api/Person.
/// Unhandled exceptions → ExceptionHandlingMiddleware → ErrorLogHelper.
/// </summary>
[Authorize]
[Route("api/Person")]
[ApiController]
public class PersonController : ApiControllerBase
{
    private readonly IPersonService _personService;

    public PersonController(IPersonService personService)
    {
        _personService = personService;
    }

    [HttpGet("SearchPersonByCNIC")]
    public async Task<IActionResult> SearchPersonByCNIC([FromQuery] string cnic)
    {
        var result = await _personService.SearchPersonByCNIC(cnic);
        return FromService(result);
    }

    [HttpGet("SearchPersonByFamilyGUID")]
    public async Task<IActionResult> SearchPersonByFamilyGUID([FromQuery] Guid FamilyGUID, [FromQuery] Guid? ProjectGuid = null)
    {
        var result = await _personService.SearchPersonByFamilyGUID(FamilyGUID, ProjectGuid);
        return FromService(result);
    }

    [HttpGet("familysummary")]
    public async Task<IActionResult> familysummary([FromQuery] Guid FamilyGUID)
    {
        var result = await _personService.familysummary(FamilyGUID);
        return FromService(result);
    }

    [HttpGet("personalinformation")]
    public async Task<IActionResult> personalinformation([FromQuery] Guid FamilyGUID, [FromQuery] Guid? ProjectGuid = null)
    {
        var result = await _personService.personalinformation(FamilyGUID, ProjectGuid);
        return FromService(result);
    }

    [HttpPost("UpdatePersonalInfo")]
    public async Task<IActionResult> UpdatePersonalInfo([FromForm] int personId, [FromForm] string? image, [FromForm] int jkid, [FromForm] int relationshipid, [FromForm] string firstName, [FromForm] string lastName, [FromForm] string? surname, [FromForm] string cnicidentificationtype, [FromForm] string cnic, [FromForm] DateTime? cnicIssueDate, [FromForm] DateTime? cnicExpiryDate, [FromForm] string? email, [FromForm] string? phone, [FromForm] string gender, [FromForm] string maritalStatus, [FromForm] DateTime? dateOfBirth, [FromForm] bool isDeceased, [FromForm] DateTime? deceasedDate, [FromForm] string? cnicfront, [FromForm] string? cnicback, [FromForm] string educationstatus, [FromForm] string youtheducationstatus, [FromForm] string workemploymentstatus, [FromForm] string disabilitystatus, [FromForm] string? disabilities, [FromForm] string substanceabusestatus, [FromForm] string? substanceabuse, [FromForm] string? btscode)
    {
        var result = await _personService.UpdatePersonalInfo(personId, image, jkid, relationshipid, firstName, lastName, surname, cnicidentificationtype, cnic, cnicIssueDate, cnicExpiryDate, email, phone, gender, maritalStatus, dateOfBirth, isDeceased, deceasedDate, cnicfront, cnicback, educationstatus, youtheducationstatus, workemploymentstatus, disabilitystatus, disabilities, substanceabusestatus, substanceabuse, btscode);
        return FromService(result);
    }

    [HttpPost("MakeHeadOfFamily")]
    public async Task<IActionResult> MakeHeadOfFamily([FromQuery] int personId)
    {
        var result = await _personService.MakeHeadOfFamily(personId);
        return FromService(result);
    }

    [HttpGet("GetPersonList")]
    public async Task<IActionResult> GetPersonList([FromQuery] string? searchTerm, [FromQuery] string sortColumn = "FirstName", [FromQuery] bool isAscending = true, [FromQuery] int page = 1)
    {
        var result = await _personService.GetPersonList(searchTerm, sortColumn, isAscending, page);
        return FromService(result);
    }

    [HttpPost("CreateQuickPerson")]
    public async Task<IActionResult> CreateQuickPerson([FromForm] string cnic, [FromForm] string identificationtype, [FromForm] string firstname, [FromForm] string lastname, [FromForm] string? surname, [FromForm] string? email, [FromForm] string? phone, [FromForm] string gender, [FromForm] int jkid, [FromForm] string maritalstatus, [FromForm] DateTime? dob, [FromForm] Guid familyguid)
    {
        var result = await _personService.CreateQuickPerson(cnic, identificationtype, firstname, lastname, surname, email, phone, gender, jkid, maritalstatus, dob, familyguid);
        return FromService(result);
    }

    //[HttpPost("ImportHouseholdData")]
    //public async Task<IActionResult> ImportHouseholdData([FromBody] string? rawText, [FromQuery] string? rawTextQuery = null)
    //{
    //    var result = await _personService.ImportHouseholdData(rawText, rawTextQuery);
    //    return FromService(result);
    //}
}
