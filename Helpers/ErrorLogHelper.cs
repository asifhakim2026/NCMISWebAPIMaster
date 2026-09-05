using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using NCMIS.Models;
using NCMISAPI.Data;

namespace NCMISAPI.Helpers;

/// <summary>
/// Persists Error / Warning / Information rows to dbo.ErrorLogs.
/// Uses a fresh DbContext scope so logging never interferes with the caller's unit of work.
/// Secondary failures are swallowed.
/// </summary>
public class ErrorLogHelper
{
    private const int MaxStackTraceLength = 4000;
    private const int MaxInnerExceptionLength = 2000;
    private const int MaxFileNameLength = 500;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ErrorLogHelper(
        IServiceScopeFactory scopeFactory,
        IHttpContextAccessor httpContextAccessor)
    {
        _scopeFactory = scopeFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    public void LogError(
        string errorDescription,
        Exception? exception = null,
        string? controllerName = null,
        string? className = null,
        string? methodName = null,
        string? moduleName = null,
        string? userName = null,
        string? fileName = null,
        string? requestPath = null,
        object? additionalData = null,
        [CallerMemberName] string callerMemberName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
    {
        Write(
            type: "Error",
            errorDescription: errorDescription,
            exception: exception,
            controllerName: controllerName,
            className: className,
            methodName: methodName,
            moduleName: moduleName,
            userName: userName,
            fileName: fileName,
            requestPath: requestPath,
            additionalData: additionalData,
            callerMemberName: callerMemberName,
            callerFilePath: callerFilePath,
            callerLineNumber: callerLineNumber);
    }

    public void LogWarning(
        string errorDescription,
        Exception? exception = null,
        string? controllerName = null,
        string? className = null,
        string? methodName = null,
        string? moduleName = null,
        string? userName = null,
        string? fileName = null,
        string? requestPath = null,
        object? additionalData = null,
        [CallerMemberName] string callerMemberName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
    {
        Write(
            type: "Warning",
            errorDescription: errorDescription,
            exception: exception,
            controllerName: controllerName,
            className: className,
            methodName: methodName,
            moduleName: moduleName,
            userName: userName,
            fileName: fileName,
            requestPath: requestPath,
            additionalData: additionalData,
            callerMemberName: callerMemberName,
            callerFilePath: callerFilePath,
            callerLineNumber: callerLineNumber);
    }

    public void LogInformation(
        string errorDescription,
        string? controllerName = null,
        string? className = null,
        string? methodName = null,
        string? moduleName = null,
        string? userName = null,
        string? fileName = null,
        string? requestPath = null,
        object? additionalData = null,
        [CallerMemberName] string callerMemberName = "",
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0)
    {
        Write(
            type: "Information",
            errorDescription: errorDescription,
            exception: null,
            controllerName: controllerName,
            className: className,
            methodName: methodName,
            moduleName: moduleName,
            userName: userName,
            fileName: fileName,
            requestPath: requestPath,
            additionalData: additionalData,
            callerMemberName: callerMemberName,
            callerFilePath: callerFilePath,
            callerLineNumber: callerLineNumber);
    }

    private void Write(
        string type,
        string errorDescription,
        Exception? exception,
        string? controllerName,
        string? className,
        string? methodName,
        string? moduleName,
        string? userName,
        string? fileName,
        string? requestPath,
        object? additionalData,
        string callerMemberName,
        string callerFilePath,
        int callerLineNumber)
    {
        try
        {
            var http = _httpContextAccessor.HttpContext;
            var resolvedUser = userName
                ?? http?.User?.FindFirst("UserName")?.Value
                ?? http?.User?.Identity?.Name
                ?? "Unknown";

            var resolvedPath = requestPath
                ?? (http?.Request != null
                    ? $"{http.Request.Path}{http.Request.QueryString}"
                    : null);

            var sourceFile = Truncate(
                !string.IsNullOrWhiteSpace(fileName)
                    ? fileName
                    : Path.GetFileName(callerFilePath),
                MaxFileNameLength);

            var description = errorDescription ?? string.Empty;
            if (exception != null && !description.Contains(exception.Message, StringComparison.Ordinal))
            {
                description = string.IsNullOrEmpty(description)
                    ? exception.Message
                    : $"{description} | {exception.Message}";
            }

            var entry = new ErrorLog
            {
                Type = type,
                ControllerName = Truncate(controllerName, 200),
                ClassName = Truncate(
                    !string.IsNullOrWhiteSpace(className)
                        ? className
                        : DeriveClassName(callerFilePath),
                    200),
                MethodName = Truncate(
                    !string.IsNullOrWhiteSpace(methodName)
                        ? methodName
                        : callerMemberName,
                    200),
                ErrorDescription = description,
                LineNumber = callerLineNumber > 0 ? callerLineNumber : null,
                CreatedAt = DateTime.Now,
                UserName = Truncate(resolvedUser, 300),
                ModuleName = Truncate(moduleName, 200),
                ExceptionType = Truncate(exception?.GetType().FullName, 500),
                StackTrace = Truncate(exception?.StackTrace, MaxStackTraceLength),
                InnerException = Truncate(exception?.InnerException?.ToString(), MaxInnerExceptionLength),
                FileName = sourceFile,
                RequestPath = Truncate(resolvedPath, 1000),
                MachineName = Truncate(Environment.MachineName, 200),
                AdditionalData = SerializeAdditionalData(additionalData)
            };

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<NcmisDbContext>();
            db.ErrorLogs.Add(entry);
            db.SaveChanges();
        }
        catch
        {
            // Never let logging break the calling operation.
        }
    }

    private static string? SerializeAdditionalData(object? additionalData)
    {
        if (additionalData == null)
            return null;

        if (additionalData is string s)
            return s;

        try
        {
            return JsonSerializer.Serialize(additionalData, JsonOptions);
        }
        catch
        {
            return additionalData.ToString();
        }
    }

    private static string? DeriveClassName(string? callerFilePath)
    {
        if (string.IsNullOrWhiteSpace(callerFilePath))
            return null;

        return Path.GetFileNameWithoutExtension(callerFilePath);
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return value.Length <= maxLength
            ? value
            : value[..maxLength];
    }
}
