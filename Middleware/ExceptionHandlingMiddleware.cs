using System.Net;
using System.Text.Json;
using NCMISAPI.DTOs;
using NCMISAPI.Exceptions;
using NCMISAPI.Helpers;

namespace NCMISAPI.Middleware;

public class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;

        var (statusCode, message) = exception switch
        {
            AppException appEx => (appEx.StatusCode, appEx.Message),
            UnauthorizedAccessException => ((int)HttpStatusCode.Unauthorized, "Unauthorized."),
            KeyNotFoundException => ((int)HttpStatusCode.NotFound, exception.Message),
            ArgumentException => ((int)HttpStatusCode.BadRequest, exception.Message),
            _ => ((int)HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        if (statusCode >= 500)
        {
            _logger.LogError(
                exception,
                "Unhandled exception. TraceId={TraceId} Path={Path}",
                traceId,
                context.Request.Path);
        }
        else
        {
            _logger.LogWarning(
                exception,
                "Handled exception. TraceId={TraceId} Status={StatusCode} Path={Path}",
                traceId,
                statusCode,
                context.Request.Path);
        }

        try
        {
            var errorLog = context.RequestServices.GetService<ErrorLogHelper>();
            if (errorLog != null)
            {
                var controller = context.GetRouteData()?.Values["controller"]?.ToString();
                var action = context.GetRouteData()?.Values["action"]?.ToString();
                var description = statusCode >= 500
                    ? $"Unhandled exception. TraceId={traceId}"
                    : $"Handled exception Status={statusCode}. TraceId={traceId}";

                // Controller/action from route data — single catch-all for all controllers
                // (no per-action ExecuteAsync wrappers). Services may also log separately.
                if (statusCode >= 500)
                {
                    errorLog.LogError(
                        description,
                        exception,
                        controllerName: controller,
                        className: controller,
                        methodName: action,
                        moduleName: "API",
                        requestPath: context.Request.Path,
                        additionalData: new { traceId, statusCode });
                }
                else
                {
                    errorLog.LogWarning(
                        description,
                        exception,
                        controllerName: controller,
                        className: controller,
                        methodName: action,
                        moduleName: "API",
                        requestPath: context.Request.Path,
                        additionalData: new { traceId, statusCode });
                }
            }
        }
        catch
        {
            // never break error response path
        }

        if (context.Response.HasStarted)
            return;

        context.Response.Clear();
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new ApiErrorResponseDto
        {
            Success = false,
            Message = _environment.IsDevelopment() && statusCode >= 500
                ? exception.Message
                : message,
            TraceId = traceId,
            StatusCode = statusCode
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}
