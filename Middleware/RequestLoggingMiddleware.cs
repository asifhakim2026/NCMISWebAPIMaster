using System.Diagnostics;

namespace NCMISAPI.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var method = context.Request.Method;
        var path = context.Request.Path.Value ?? "/";
        var traceId = context.TraceIdentifier;

        _logger.LogInformation(
            "HTTP {Method} {Path} started. TraceId={TraceId}",
            method,
            path,
            traceId);

        try
        {
            await _next(context);
            stopwatch.Stop();

            _logger.LogInformation(
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms. TraceId={TraceId}",
                method,
                path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                traceId);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "HTTP {Method} {Path} failed after {ElapsedMs}ms. TraceId={TraceId}",
                method,
                path,
                stopwatch.ElapsedMilliseconds,
                traceId);

            throw;
        }
    }
}
