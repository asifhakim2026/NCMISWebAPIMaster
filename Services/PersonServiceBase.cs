using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NCMISAPI.Data;
using NCMISAPI.DTOs.Person;
using NCMISAPI.Helpers;

namespace NCMISAPI.Services;

/// <summary>Shared Person service helpers (user context + result factories + error handling).</summary>
public abstract class PersonServiceBase
{
    protected readonly NcmisDbContext _dbContext;
    protected readonly IHttpContextAccessor _httpContextAccessor;
    protected readonly ILogger _logger;
    protected readonly ErrorLogHelper _errorLogHelper;

    protected PersonServiceBase(
        NcmisDbContext dbContext,
        IHttpContextAccessor httpContextAccessor,
        ILogger logger,
        ErrorLogHelper errorLogHelper)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _errorLogHelper = errorLogHelper;
    }

    protected string CurrentUserName
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.FindFirstValue("UserName")
                   ?? user?.FindFirstValue(JwtRegisteredClaimNames.UniqueName)
                   ?? user?.Identity?.Name
                   ?? "System";
        }
    }

    protected int CurrentUserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var raw = user?.FindFirstValue("UserID")
                      ?? user?.FindFirstValue(JwtRegisteredClaimNames.Sub)
                      ?? user?.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(raw, out var id) ? id : 0;
        }
    }

    /// <summary>
    /// Persists a caught exception to dbo.ErrorLogs (and logs via ILogger).
    /// Prefer calling this from catch blocks that return a safe result instead of rethrowing.
    /// </summary>
    protected void LogAndPersistError(
        Exception ex,
        string errorDescription,
        object? additionalData = null,
        [CallerMemberName] string operation = "")
    {
        var methodName = string.IsNullOrWhiteSpace(operation) ? "Unknown" : operation;

        _logger.LogError(
            ex,
            "{ErrorDescription}. UserId={UserId}",
            errorDescription,
            CurrentUserId);

        _errorLogHelper.LogError(
            errorDescription: errorDescription,
            exception: ex,
            className: GetType().Name,
            methodName: methodName,
            moduleName: "Person",
            userName: CurrentUserName,
            additionalData: additionalData ?? new { CurrentUserId });
    }

    protected static ApiResultDto OkResult(string message, object? data = null) =>
        new() { Success = true, Message = message, Data = data };

    protected static ApiResultDto FailResult(string message, object? data = null) =>
        new() { Success = false, Message = message, Data = data };
}
