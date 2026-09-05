using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NCMISAPI.Services;

namespace NCMISAPI.Controllers;

/// <summary>
/// PersonDocumentController - HTTP layer; routes preserved under api/Person.
/// Unhandled exceptions → ExceptionHandlingMiddleware → ErrorLogHelper.
/// </summary>
[Authorize]
[Route("api/Person")]
[ApiController]
public class PersonDocumentController : ApiControllerBase
{
    private readonly IPersonDocumentService _documentService;

    public PersonDocumentController(IPersonDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpGet("SeniorCitizenFamilyGUID")]
    public async Task<IActionResult> SeniorCitizenFamilyGUID([FromQuery] Guid FamilyGUID)
    {
        var result = await _documentService.SeniorCitizenFamilyGUID(FamilyGUID);
        return FromService(result);
    }

    [HttpPost("SavePersonSeniorCard")]
    public async Task<IActionResult> SavePersonSeniorCard([FromForm] int personId, [FromForm] string cardNumber, [FromForm] DateTime? issueDate, [FromForm] DateTime? expiryDate, [FromForm] string issuerType, [FromForm] string issuedBy, [FromForm] string? amenities, [FromForm] string? description)
    {
        var result = await _documentService.SavePersonSeniorCard(personId, cardNumber, issueDate, expiryDate, issuerType, issuedBy, amenities, description);
        return FromService(result);
    }

    [HttpPost("MarkSeniorCardInactive")]
    public async Task<IActionResult> MarkSeniorCardInactive([FromForm] int id, [FromForm] string reason, [FromForm] string? description)
    {
        var result = await _documentService.MarkSeniorCardInactive(id, reason, description);
        return FromService(result);
    }

    [HttpPost("SavePersonAttachment")]
    public async Task<IActionResult> SavePersonAttachment([FromForm] int personId, [FromForm] string attachmentName, [FromForm] string attachmentUrl)
    {
        var result = await _documentService.SavePersonAttachment(personId, attachmentName, attachmentUrl);
        return FromService(result);
    }

    [HttpGet("FamilyAttachmentFamilyGUID")]
    public async Task<IActionResult> FamilyAttachmentFamilyGUID([FromQuery] Guid FamilyGUID)
    {
        var result = await _documentService.FamilyAttachmentFamilyGUID(FamilyGUID);
        return FromService(result);
    }
}
