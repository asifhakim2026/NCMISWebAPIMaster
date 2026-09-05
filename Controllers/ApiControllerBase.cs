using Microsoft.AspNetCore.Mvc;
using NCMISAPI.Services;

namespace NCMISAPI.Controllers;

/// <summary>
/// Thin shared base for <see cref="PersonServiceResult"/> → <see cref="IActionResult"/> mapping.
/// Controller exceptions are not caught here — <c>ExceptionHandlingMiddleware</c> logs them
/// via ErrorLogHelper (dbo.ErrorLogs). Services still perform their own ErrorLogHelper logging.
/// </summary>
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult FromService(PersonServiceResult result) =>
        StatusCode(result.StatusCode, result.Body);
}
