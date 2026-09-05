using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NCMISAPI.DTOs;
using NCMISAPI.Services;

namespace NCMISAPI.Controllers;

/// <summary>
/// Master data and utility endpoints (file upload, etc.).
/// Unhandled exceptions → ExceptionHandlingMiddleware → ErrorLogHelper.
/// </summary>
[Authorize(Policy = "LoggedInPolicy")]
[Route("api/[controller]")]
[ApiController]
public class MasterController : ControllerBase
{
    private readonly IFileStorageService _fileStorageService;

    public MasterController(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    /// <summary>
    /// Uploads a file to the CDN folder and returns the relative path.
    /// </summary>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [Produces("application/json")]
    [ProducesResponseType(typeof(FileUploadResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadFile(
        [FromForm] FileUploadFormDto form,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _fileStorageService.SaveFileAsync(form.File, cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Downloads a file from the configured CDN folder. Requires JWT.
    /// Pass either full relative path (/cdn/name.jpg) or file name only.
    /// </summary>
    [HttpGet("download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DownloadFile(
        [FromQuery] string? path = null,
        [FromQuery] string? fileName = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var key = !string.IsNullOrWhiteSpace(path) ? path : fileName;
            if (string.IsNullOrWhiteSpace(key))
                return BadRequest(new { success = false, message = "Provide path or fileName." });

            var file = await _fileStorageService.OpenReadAsync(key, cancellationToken);
            if (file is null)
                return NotFound(new { success = false, message = "File not found." });

            // Caller disposes via FileStreamResult when the response completes.
            return File(file.Stream, file.ContentType, file.FileName);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}
